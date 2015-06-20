// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

module fracas

open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.Patterns
open System
open System.ComponentModel
open System.Windows.Input

type IFieldBacker =
    abstract member Updated : IEvent<obj> with get

/// Implements INotifyPropertyChanged for use in observable models.
type NotifyBase() =

    let propertyChanged = Event<_, _>()
    
    interface INotifyPropertyChanged with
        [<CLIEvent>]
        member __.PropertyChanged = propertyChanged.Publish

    member internal __.MakeField<'T>(propertyExpr, ?initialValue: 'T) =
        new FieldBacker<'T>(__, propertyExpr, initialValue)

    member internal __.MakeCommand<'T>(canExecuteHandler, executeHandler, notifyOnFieldUpdate) =
        CommandBacker<'T>(canExecuteHandler, executeHandler, notifyOnFieldUpdate)

    // Used by FieldBacker to update listeners
    member internal x.NotifyPropertyChanged(propertyName) =
        propertyChanged.Trigger(x, PropertyChangedEventArgs(propertyName))

// Backs fields that cause updates in ObservableModel.
and FieldBacker<'T>(om: NotifyBase, propertyExpr, initialValue: 'T option) =

    let mutable value = match initialValue with
                        | Some t -> t
                        | None -> Unchecked.defaultof<'T>
    let internalUpdateEvent = new Event<obj>()
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

    interface IFieldBacker with
        member __.Updated: IEvent<obj> = 
            internalUpdateEvent.Publish

    member internal x.Updated = (x :> IFieldBacker).Updated

/// Backs commands; note that the handlers take 'T option so use your pattern matching.
and CommandBacker<'T>(canExececuteHandler: 'T option -> bool,
                      executeHandler: 'T option -> unit,
                      notifyOnFieldUpdate: IFieldBacker list) as x =

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

let mkField<'T> propertyExpr (initialValue: 'T) (obs: NotifyBase) = obs.MakeField (propertyExpr, initialValue)

let mkCommand<'T> canExecuteHandler executeHandler (notifyOnFieldUpdate: IFieldBacker list) (obs: NotifyBase) =
    
    obs.MakeCommand<'T> (canExecuteHandler, executeHandler, notifyOnFieldUpdate)

let notifyAllChanged (obs: NotifyBase) = obs.NotifyPropertyChanged ""
