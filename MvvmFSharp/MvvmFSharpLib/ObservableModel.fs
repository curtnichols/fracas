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

    member x.setProperty<'T>(field: 'T byref, newValue: 'T, propertyExpr) =
        match propertyExpr with
        | PropertyGet(_, propOrValInfo, _) ->
            if Object.Equals(field, newValue) then false
            else 
                field <- newValue
                propertyChanged.Trigger(x, PropertyChangedEventArgs(propOrValInfo.Name))
                true
        | _ -> failwith "Unexpected expression type; needs PropertyGet"
