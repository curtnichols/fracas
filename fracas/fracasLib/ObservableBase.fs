// The MIT License (MIT)
// 
// Copyright (c) 2013-2014 Curt Nichols
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

module fracas

open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.Patterns
open System
open System.ComponentModel
open System.Windows.Input

/// Implements INotifyPropertyChanged for use in observable models.
type ObservableBase() =

    let propertyChanged = Event<_, _>()
    
    interface INotifyPropertyChanged with 
        [<CLIEvent>]
        member __.PropertyChanged = propertyChanged.Publish

    member internal x.MakeField<'T>(propertyExpr, ?initialValue: 'T) = // TODO would rather have protected mfunc or func
        new FieldBacker<'T>(x, propertyExpr, initialValue)

    member internal __.MakeCommand<'T>(canExecuteHandler, executeHandler, notifyOnFieldUpdate) =
        CommandBacker<'T>(canExecuteHandler, executeHandler, notifyOnFieldUpdate)

    // Used by FieldBacker to update listeners
    member internal x.NotifyPropertyChanged(propertyName) =
        propertyChanged.Trigger(x, PropertyChangedEventArgs(propertyName))

// Backs fields that cause updates in ObservableModel.
and FieldBacker<'T>(om: ObservableBase, propertyExpr, initialValue: 'T option) =

    let mutable value = match initialValue with
                        | Some t -> t
                        | None -> Unchecked.defaultof<'T>
    let internalUpdateEvent = new Event<'T>()
    let propertyName = 
        match propertyExpr with
        | PropertyGet(_, propOrValInfo, _) -> propOrValInfo.Name
        | _ -> failwith "Unexpected expression type; needs PropertyGet"

    let setValue newValue =
        if Object.Equals(value, newValue) then false
        else 
            value <- newValue
            om.NotifyPropertyChanged(propertyName)
            internalUpdateEvent.Trigger newValue
            true

    member __.Value
        with get() = value
        and set newValue = setValue newValue |> ignore

    /// Sets the value; use when a return value is required to determine if the value has changed.
    member __.Set newValue = setValue newValue

    member internal __.Updated = internalUpdateEvent.Publish // For internal clients like CommandBacker

/// Backs commands; note that the handlers take 'T option so use your pattern matching.
and CommandBacker<'T>(canExececuteHandler: 'T option -> bool,
                      executeHandler: 'T option -> unit,
                      notifyOnFieldUpdate: FieldBacker<'T> list) as x =

    let canExecuteChanged = Event<_, _>()
    do
        let notifyUpdate = fun _ -> x.NotifyCanExecuteChanged()
        for backer in notifyOnFieldUpdate do
            backer.Updated.Add(notifyUpdate)
    
    interface ICommand with
        [<CLIEvent>]
        member __.CanExecuteChanged = canExecuteChanged.Publish

        member __.CanExecute parameter =
            match parameter with
            | :? 'T -> canExececuteHandler (Some (parameter :?> 'T))
            | p when p = null -> canExececuteHandler None
            | _ -> false

        member __.Execute parameter =
            match parameter with
            | :? 'T -> executeHandler (Some (parameter :?> 'T))
            | p when p = null -> executeHandler None
            | _ -> () // Unexpected parameter type // TODO what to do here?

    member x.ICommand = x :> ICommand

    member x.NotifyCanExecuteChanged() = canExecuteChanged.Trigger(x, EventArgs.Empty)

// Functions

let mkField<'T> propertyExpr (initialValue: 'T) (obs: ObservableBase) = obs.MakeField (propertyExpr, initialValue)

let mkCommand<'T> canExecuteHandler executeHandler (notifyOnFieldUpdate: FieldBacker<'T> list) (obs: ObservableBase) =
    
    obs.MakeCommand<'T> (canExecuteHandler, executeHandler, notifyOnFieldUpdate)

let notifyAllChanged (obs: ObservableBase) = obs.NotifyPropertyChanged ""
