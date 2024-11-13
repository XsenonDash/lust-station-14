﻿using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface.XAML;

namespace Content.Client._Sunrise.Medical.Surgery;
// Based on the RMC14.
// https://github.com/RMC-14/RMC-14
[GenerateTypedNameReferences]
public sealed partial class SurgeryStepButton : ChoiceControl
{
    public EntityUid Step { get; set; }

    public SurgeryStepButton() => RobustXamlLoader.Load(this);
}
