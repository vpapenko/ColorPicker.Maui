# Maui ColorPicker Developer's Guide

## Chapter 2: ColorPicker Library
This document describes the architecture of the ColorPicker project and many of the design decisions made when structuring the library.

### Project Structure
This Project consists a collection of base classes in the BaseClasses folder and several custom color picker and slider controls in the Controls folder. The Controls folder has subfolder named for each control where the control code id defined. There is a Sliders subfolder where the sliders are defined. It may prove advisable to break out the sliders into separate folders.

### Namepaces
To simplify references to code in the ColorPicker library, all code in the library is defined to reside in the **ColorPicker** namespace. This removes several unnecessary dependencies that complicates maintenance.

### Drawables
This library is written based upon the Microsoft.Maui.Graphics library which implements drawing code by supplying the control with the implementation of IDrawable, which the base class caches for later use. 

> Given a control named Custom, a custom control that draws it's own UI, the drawing code is located in the control's folder alongside Custom.cs as ControlDrawable.cs.

### Partial Classes  
C# code can become hard to read  quickly, based upon how complex it is. The C# language offers the ability to create classes in different files by specifying that the class definition in each of those files is a *partial* definition; that is, the full class definition consists of the result of the compiler aggregating all files (regardless of file name) containing one or more class declarations with the *same* class name that use the "partial" keyword. The ColorPicker library uses partial classes in two ways: for separation of concerns and for implementation of platform-dependent code (if any).

#### Partial Classes Help Manage Separation of Concerns
As noted above C# code can get complicated and hard to read when a class gets large. One element of Maui Custom Controls is **data binding** which includes the definition of **BindableProperties**, a mechanism provided to make communication between XAML and C# possible when a property that the UI depends upon changes. When a control contains a large number of BindableProperties, the code can get quite cluttered. The ColorPicker library implements the following convention:

> Given a control named Custom, basic logic, including the constructor, is contained in a file named Custom.cs. BindableProperty declarations including support code is contained in a file named CustomProperties.cs.

#### A Brief Detour into Support for Platform-Dependent Code
Platform-dependent code can be implemented as follows:

| Implementing Platform-Dependent Code        |
| :----------------------------------------------------- |
| With Compiler directives (#ifdef/#elif#endif)               |
| Via the Platform folder with platform-specific sub-folders |
| With a Platform-specific file naming convention               |

The first two mechanisms are the most commonly used. Compiler directives are appropriate for small amounts of code but are less comprehensible after more than a few lines of code. For example, something that might take two or three lines of code for Android, might require a whole page of code for a different platform. For small amounts of code, this is the simplest approach while still intelligible. For more complex scenarios, the other two mechanisms are more appropriate.

#### Partial Classes Are a Light-Weight Way to Handle Platform-Dependent Code
The third option, platform-specific file naming, is the least commonly used yet a perfectly viable option and lighter-weight than the using the Platform folder as described above, especially for libraries. There is no Platform folder in the ColorPicker library but, like the file-naming approach for BindableProperties, the library uses partial classes for platform-dependent code. In fact, this following approach can be combined with the BindableProperty naming convention. The file naming convention is:

> Given a control named Custom contained in a file named Custom.cs and, optionally, BindableProperty declarations for Custom contained in a file named CustomProperties.cs. If extensive platform-dependent code is required, it will be stored in a file named Custom.\<platform\>.cs, where \<platform\> is Android, iOS, MacCatalyst, Tizen, or Windows. There will be one file for each platform for which platform-dependent code is required. **Note that Mac Catalyst and Tizen code is not yet implemented.**

As a hypothetical example, let's assume that there is a control named Picker. It's main declaration is in Picker.cs and its BindableProperties, if any, are in PickerProperties.cs. Let's say that there is a need on Windows to use a native API to implement an element on Picker. In that case, the code would be in Picker.Windows.cs. 

#### Implementing File Name Specific Platform-Dependent Code
The Microsoft .NET Maui documentation on *multi-targeting* defines this mechanism at [https://docs.microsoft.com/en-us/dotnet/maui/platform-integration/configure-multi-targeting](link_url)

Search for the section **Configure filename-based multi-targeting**. In essence, it requires the addition of code to the Project file to support compilation of files based upon platform. This code has been added to the ColorPicker.csproj file. Note that it currently assumes that code for iOS and Mac Catalyst use the same platform-dependent code. If this changes, then the Mac Catalyst code will need to be broken out.












