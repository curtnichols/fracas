# fracas

**fracas** is a minimalist MVVM library built in F# for F#. Please see the sample "test" application and view model for examples.

Release notes are [here](release-history.md).

## History

Some time ago, when Windows 8 was a thing, I worked on a personal project, a "Metro-style" application. It was an additive synthesis app, for building audio sounds. While doing so, I built a tiny MVVM library in support of that application.

The library was intentionally quite small, as I had some specific functionality I wanted to explore, including:
* Error handling.
* Constraining input values.
* Properties derived from other properties.

My language of choice for this app was F#, and it was a nice, functional way to build waveform generators. And to build MVVM models.

You see, Metro apps could be written in F# back then.

As of the release of Windows 10 UWP applications, it appears that F# is not supported, not for actual store delivery, and the original app has faded from my interests.

At my job, when we started building our models in F#, I published the **fracas** code and made use of it. I think it works just fine for our limited needs, and will continue to do so.

As of late November, 2018, **I've decided to archive this repo.** No active work to be done, it will likely be used commercially for some time. At the time of this writing, the plan is to check on the required version of FSharp.Core, possibly downgrading it to 4.5.0; build and test a new release; publish the new release on github and nuget; and archive this repo. Presuming .NET Standard doesn't break it, that's were it will likely remain.

Curt<br/>
November 28, 2018

## Goals

Primary goals of this project are:
* Explore how to build MVVM support in F#, specifically for use in XAML applications.
* Explore F# error handling in MVVM
* Explore constraint of values in MVVM
* Explore asynchronous application of values in MVVM
