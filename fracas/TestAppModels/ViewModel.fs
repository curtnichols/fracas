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

type ViewModel(model: Model) as self =
    inherit fracas.ObservableBase()

    // EXAMPLE: creates fields that on update cause notifications through ObservableBase.
    let requestedVolume = self |> fracas.mkField <@ self.RequestedVolume @> model.CurrentSettings.Volume
    let requestedPan = self |> fracas.mkField <@ self.RequestedPan @> model.CurrentSettings.Pan
    let isVolumeConstrained = self |> fracas.mkField <@ self.IsVolumeConstrained @> false

    // EXAMPLE: creates a command backer with a "can execute" handler, an "execute" handler,
    // and specifies field backer(s) that cause the command to update its executable status.
    let resetPanCommand = self |> fracas.mkCommand (fun _ -> requestedPan.Value <> 0.0)
                                                   (fun _ -> self.RequestedPan <- 0.0)
                                                   [requestedPan]

    member x.Model with get() = model

    // EXAMPLE: exposes a command backer as ICommand
    member x.ResetPanCommand with get() = resetPanCommand.ICommand

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

    member x.RequestedPan
        with get() = requestedPan.Value
        and set newValue =
            if requestedPan.Set newValue then
                match model.ApplySettings {model.LastRequestedSettings with Pan = newValue} with
                | AudioSettingsAsIs s | AudioSettingsConstrained s -> ()
                | Error msg -> ()
