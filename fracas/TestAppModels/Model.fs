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

namespace TestAppModels

type ValidationResult =
    | Success
    | Error of description: string

type AudioSettings = { Volume: float; Pan: float }
with
    member x.Validate() =
        match x.Volume with
        | v when v < 0.0 -> Error("Negative Volume is not supported")
        | _ -> // We'll constrain to 0.0 - 1.0
            match x.Pan with
            | v when v < -1.0 || v > 1.0 -> Error("Invalid Pan newValue")
            | _ -> Success

type AppliedSettingsResult =
    | AudioSettingsAsIs of settings: AudioSettings
    | AudioSettingsConstrained of settings: AudioSettings
    | Error of error: string

type Model(volume: float, pan: float) as self =
    inherit fracas.NotifyBase ()

    // EXAMPLE: creates fields that on update cause notifications through ObservableBase.
    let currentSettings = self |> fracas.mkField <@ self.CurrentSettings @> { Volume = volume; Pan = pan }
    let lastRequestedSettings = self |> fracas.mkField <@ self.LastRequestedSettings @> { Volume = volume; Pan = pan }
    
    // EXAMPLE: makes use of backing fields to implement properties with notifications.
    member x.CurrentSettings
        with get() = currentSettings.Value
        and set newValue = currentSettings.Value <- newValue

    member x.LastRequestedSettings
        with get() = lastRequestedSettings.Value
        and set newValue = lastRequestedSettings.Value <- newValue

    member x.ApplySettings (settings: AudioSettings) =
        match settings.Validate() with
        | ValidationResult.Error msg -> Error(msg)
        | _ ->
            let constrained = Model.constrain settings
            match constrained with
            | AudioSettingsAsIs s | AudioSettingsConstrained s ->
                let delayedApplicationSimulation =
                    async {
                        // Not real-world code: these updates can be out-of-sequence.
                        do! Async.Sleep(1000)
                        x.CurrentSettings <- s
                    }
                delayedApplicationSimulation |> Async.Start
                x.LastRequestedSettings <- s
            | Error desc -> ()

            constrained

    static member constrain settings =
        if settings.Volume > 1.0 then AudioSettingsConstrained({settings with Volume = 1.0})
        else AudioSettingsAsIs(settings)
