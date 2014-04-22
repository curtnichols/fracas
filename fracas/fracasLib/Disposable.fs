module Disposable

open System

/// Invokes 'first', then disposes of the disposables, afterward setting disposed to true.
let disposeWith (disposed: bool ref) (disposables: IDisposable list) (first: unit -> unit) =
    first()
    disposables |> List.iter (fun d -> d.Dispose())
    disposed := true

/// Disposes of the disposables, afterward setting disposed to true.
let dispose (disposed: bool ref) (disposables: IDisposable list) =
    (fun () -> ()) |> disposeWith disposed disposables
