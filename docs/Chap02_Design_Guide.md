# Maui ColorPicker Developer's Guide

>Please note that this document is a **Work in Progress**


## Chapter 2: ColorPicker and Slider Design Guide

### ColorPickers and Sliders
> For simplicity, when referencing both ColorPickers and Slider controls, I will use the term **picker(s)**.

A "picker" defines a bounded *colorspace* from which a user can select a specific Color, where a Color is a **Maui.Graphics.Color** object.

A **ColorPicker's** colorspace defines the range of Color values from which a user can select a Color value.

A **Slider's** colorspace defines a subset of a ColorPicker's Color values to which the Slider's value can be set.

#### The SelectedColor Property
**SelectedColor** is one of two key bindable properties each picker contains. SelectedColor specifies the current Color selected either by the user or, programmatically, as the result of picker operations described later in this document. 

#### The AttachedTo Property
The other key bindable property is **AttachedTo**. It establishes a two-way connection between pickers with the following constraints:

- A ColorPicker can be AttachedTo one other ColorPicker
- A Slider can be AttachedTo one ColorPicker
- A Slider cannot be AttachedTo a Slider
- A ColorPicker can have any number of Sliders AttachedTo it
- Neither a ColorPicker nor a Slider can be AttachedTo itself

#### The Reticle
Pickers also have an implicit property: the *Reticle*. The Reticle defines *where* within the picker's colorspace the **SelectedColor** is located; that is, The Reticle's *value* is the **SelectedColor** for that picker. 

The Reticle is shown visually as a small set of concentric circles, with an optional cross-hair, whose center is the value of the Reticle.

The Reticle can be modified via two additional Bindable Properties: ReticleRadius and ShowReticleCrossHairs. ReticalRadius defaults to 20 and ShowReticleCrossHairs defaults to false. Both Bindable Properties have a BindingMode of OneTime.



  