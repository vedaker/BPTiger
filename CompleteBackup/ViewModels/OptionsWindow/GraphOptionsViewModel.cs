﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

using RadAnalytics.DataAccess;
using RadAnalytics.Utilities;
using RadAnalytics.JAG.Interfaces;
using RadAnalytics.Models;
using System.Windows.Threading;
using System.Threading;
using System.Windows.Input;

namespace RadAnalytics.ViewModels
{
    class GraphOptionsViewModel : ObservableObject, IPageViewModel
    {
        public GraphOptionsViewModel()
        {

        }

        public RadAnalytics.Properties.GraphSettings GraphSettings
        {
            get
            {
                return Properties.GraphSettings.Default;
            }
        }

        public string Name
        {
            get
            {
                return "Graph Options Page";
            }
        }
    }
}
