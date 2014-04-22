fracas
======

This project is an exploration of loosely related F# code.

Everything below is a bit old....

Goals
-----
Primary goals of this project are:
* Explore how to build MVVM support in F#, specifically for use in XAML applications.
* Explore F# error handling in MVVM
* Explore constraint of values in MVVM
* Explore asynchronous application of values in MVVM

Solution Parts
--------------
The projects in this solution are:
* MvvmFSharp - a portable library that implements some basic MVVM support.
* TestAppModels - implements sample Models and View Models for noodling on.
* TestAppCSharp - a C# WPF application for illustrating use of the models and viewmodels;
let's face it, C# has much better tooling for WPF than F#.
