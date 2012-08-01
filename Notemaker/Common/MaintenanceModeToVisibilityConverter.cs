using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows;
using JayDev.Notemaker.Core;

namespace JayDev.Notemaker.Common
{

    public sealed class MaintenanceModeToVisibilityConverter : EnumConverter<MaintenanceMode>
    {
        public MaintenanceModeToVisibilityConverter() : base(MaintenanceMode.View) { }
    }
}
