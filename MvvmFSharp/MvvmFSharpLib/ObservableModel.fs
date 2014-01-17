namespace MvvmFSharpLib

open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.Patterns
open System
open System.ComponentModel
open System.Runtime.CompilerServices 

/// Implements INotifyPropertyChanged for use in observable models.
type ObservableBase() =
    let propertyChanged = Event<_, _>()
    
    interface INotifyPropertyChanged with 
        [<CLIEvent>]
        member x.PropertyChanged = propertyChanged.Publish

    member x.MakeField<'T>(propertyExpr, ?initialValue: 'T) = // TODO would rather have protected mfunc or func
        new FieldBacker<'T>(x, propertyExpr, initialValue)

    // Used by FieldBacker
    member internal x.NotifyPropertyChanged(propertyName) =
        propertyChanged.Trigger(x, PropertyChangedEventArgs(propertyName))

and FieldBacker<'T>(om: ObservableBase, propertyExpr, initialValue: 'T option) =
    let mutable value = match initialValue with
                        | Some t -> t
                        | None -> Unchecked.defaultof<'T>
    let propertyName = 
        match propertyExpr with
        | PropertyGet(_, propOrValInfo, _) -> propOrValInfo.Name
        | _ -> failwith "Unexpected expression type; needs PropertyGet"
    let setValue newValue =
        if Object.Equals(value, newValue) then false
        else 
            value <- newValue
            om.NotifyPropertyChanged(propertyName)
            true

    member x.Value
        with get() = value
        and set newValue = setValue newValue |> ignore

    /// Sets the value; use when a return value is required to determine if the value has changed.
    member x.Set newValue = setValue newValue
