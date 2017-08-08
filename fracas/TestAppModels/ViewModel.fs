// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

namespace TestAppModels

open System
open System.Threading

type ViewModel(model: Model) as self =
    inherit fracas.NotifyBase ()

    // EXAMPLE: creates fields that on update cause notifications through ObservableBase.
    let requestedVolume = self |> fracas.mkField <@ self.RequestedVolume @> model.CurrentSettings.Volume
    let requestedPan = self |> fracas.mkField <@ self.RequestedPan @> model.CurrentSettings.Pan
    let isVolumeConstrained = self |> fracas.mkField <@ self.IsVolumeConstrained @> false

    let derived1 =
            self |> fracas.mkDerivedField <@ self.Derived1 @> [ requestedVolume ]
                        (fun () -> requestedVolume.Value)
    // Second-order derivation
    let derivedVolumeAsAString =
            self |> fracas.mkDerivedField <@ self.DerivedVolumeAsAString @> [ derived1 ]
                        (fun () -> sprintf "Derived volume as a string: %.2f" derived1.Value)

    // EXAMPLE: creates a command backer with a "can execute" handler, an "execute" handler,
    // and specifies field backer(s) that cause the command to update its executable status.
    let resetPanCommand = self |> fracas.mkCommand (fun _ -> requestedPan.Value <> 0.0)
                                                   (fun _ -> self.RequestedPan <- 0.0)
                                                   [requestedPan]

    let asyncTestFunc f _ = async { do! async { self.ErrorRecoveryText <- "Running..." }
                                    do! f }

    let slowAsyncCommand =
        self |>
        fracas.mkAsyncCommand (fun _ -> true)
                              (asyncTestFunc (Async.Sleep(2000)))
                              (fun _ -> self.ErrorRecoveryText <- "Done.")
                              (fun exn -> self.ErrorRecoveryText <- sprintf "Exception: %s" exn.Message)
                              (fun cexn -> self.ErrorRecoveryText <- sprintf "Exception: %s" cexn.Message)
                              (fun _ -> None)
                              []

    let asyncExceptionCommand =
        self |>
        fracas.mkAsyncCommand (fun _ -> true)
                              (asyncTestFunc (async {
                                do! Async.Sleep(1000)
                                failwith "async cmd with exception" }))
                              (fun _ -> self.ErrorRecoveryText <- "Done.")
                              (fun exn -> self.ErrorRecoveryText <- sprintf "Exception: %s" exn.Message)
                              (fun cexn -> self.ErrorRecoveryText <- sprintf "Exception: %s" cexn.Message)
                              (fun _ -> None)
                              []

    // This to-be-cancelled command is not friendly with a CancellationTokenSource that is
    // set to cancel after T time period, as it's not known when the command will be exeucted.
    // The time period tends to expire before the button is pushed or at another indeterminate time.
    let asyncCancelledCommand =
        let getCancellationTokenWithTimeout () = 
            let cts = new CancellationTokenSource()
            cts.CancelAfter(TimeSpan.FromSeconds(2.0))
            Some cts.Token

        self |>
        fracas.mkAsyncCommand (fun _ -> true)
                              (asyncTestFunc (async {
                                do! Async.Sleep(20 * 1000)
                                do! async { self.ErrorRecoveryText <- "Done!?" }
                              }))
                              (fun _ -> self.ErrorRecoveryText <- "Done.")
                              (fun exn -> self.ErrorRecoveryText <- sprintf "Exception: %s" exn.Message)
                              (fun cexn -> self.ErrorRecoveryText <- sprintf "Exception: %s" cexn.Message)
                              getCancellationTokenWithTimeout
                              []

    let errorRecoveryText = self |> fracas.mkField <@ self.ErrorRecoveryText @> ""

    member x.Model with get() = model

    // EXAMPLE: exposes a command backer as ICommand
    member x.ResetPanCommand with get() = resetPanCommand.ICommand

    member x.SlowAsyncCommand with get() = slowAsyncCommand.ICommand

    member x.AsyncExceptionCommand with get() = asyncExceptionCommand.ICommand

    member x.AsyncCancelledCommand with get() = asyncCancelledCommand.ICommand

    // EXAMPLE: makes use of backing fields to implement properties with notifications.
    member x.IsVolumeConstrained
        with get() = isVolumeConstrained.Value
        and private set newValue = isVolumeConstrained.Value <- newValue

    member x.RequestedVolume
        with get() = requestedVolume.Value
        and set newValue =
            if requestedVolume.Set newValue then
                match model.ApplySettings {model.LastRequestedSettings with Volume = newValue} with
                | AudioSettingsAsIs s -> x.IsVolumeConstrained <- false
                | AudioSettingsConstrained s -> x.IsVolumeConstrained <- true
                | Error msg -> ()

    member x.Derived1
        with get() = derived1.Value

    member x.DerivedVolumeAsAString
        with get() =
            // Derived field backers require a 'generateValue' function in order to keep the same
            // 'backer.Value' usage as common field backers. This also allows us to keep a reference
            // to the backer in the property implementation, avoiding GC-ing the backer.
            derivedVolumeAsAString.Value

    member x.RequestedPan
        with get() = requestedPan.Value
        and set newValue =
            if requestedPan.Set newValue then
                match model.ApplySettings {model.LastRequestedSettings with Pan = newValue} with
                | AudioSettingsAsIs s | AudioSettingsConstrained s -> ()
                | Error msg -> ()

    member x.ErrorRecoveryText
        with get() = errorRecoveryText.Value
        and set newValue = errorRecoveryText.Value <- newValue
