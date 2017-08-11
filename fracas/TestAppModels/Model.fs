// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

namespace TestAppModels

type ValidationResult =
    | Success
    | Error of description: string

/// Settings used in the sample application.
type AudioSettings = { Volume: float; Pan: float }
with
    member x.Validate() =
        match x.Volume with
        | v when v < 0.0 -> Error("Negative Volume is not supported")
        | _ -> // We'll constrain to 0.0 - 1.0
            match x.Pan with
            | v when v < -1.0 || v > 1.0 -> Error("Invalid Pan newValue")
            | _ -> Success

/// Represents the outcome of attempting to apply settings to the model.
/// Upon applying settings, one of three outcomes may ensue:
/// a) the settings are applied as requested.
/// b) the settings are constrained to ensure they fall within limits.
/// c) The settings are outright rejected.
type AppliedSettingsResult =
    | AudioSettingsAsIs of settings: AudioSettings
    | AudioSettingsConstrained of settings: AudioSettings
    | Error of error: string

module private Details =

    let constrain settings =
        if settings.Volume > 1.0 then AudioSettingsConstrained({settings with Volume = 1.0})
        else AudioSettingsAsIs(settings)

/// Model for the application. You can apply settings via <c>ApplySettings</c>.
/// In this case, both <c>CurrentSettings</c> and
/// <c>LastRequestedSettings</c> have private setters, as
/// <c>ApplySettings</c> is used to apply new settings to the model.
type Model(initialSettings : AudioSettings) as self =
    inherit fracas.NotifyBase ()

    // EXAMPLE: creates fields that on update cause notifications through ObservableBase.
    let currentSettings = self |> fracas.mkField <@ self.CurrentSettings @> initialSettings
    let lastRequestedSettings = self |> fracas.mkField <@ self.LastRequestedSettings @> initialSettings
    
    // EXAMPLE: makes use of backing fields to implement properties with notifications.
    member x.CurrentSettings
        with get() = currentSettings.Value
        and private set newValue = currentSettings.Value <- newValue

    member x.LastRequestedSettings
        with get() = lastRequestedSettings.Value
        and private set newValue = lastRequestedSettings.Value <- newValue

    member x.ApplySettings (settings: AudioSettings) : AppliedSettingsResult =
        match settings.Validate() with
        | ValidationResult.Error msg -> Error(msg)
        | _ ->
            let constrained = Details.constrain settings
            match constrained with
            | AudioSettingsAsIs s | AudioSettingsConstrained s ->
                let delayedApplicationSimulation =
                    async {
                        // Not good, real-world code: multiple calls to
                        // ApplySettings can be applied out-of-sequence.
                        do! Async.Sleep(1000)
                        x.CurrentSettings <- s
                    }
                delayedApplicationSimulation |> Async.Start
                x.LastRequestedSettings <- s
            | Error desc -> ()

            constrained
