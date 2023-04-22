using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIDControl
{
    public class CustomControl : IControl
    {
        public double OutputValue { get; private set; } = 0;

        public double ActualValue { get; private set; } = 0;

        public double TargetValue { get; private set; } = 10;

        public void Output(double output)
        {
            ActualValue = output;
        }

        public void Reset()
        {
            OutputValue = 0;
            ActualValue = 0;
            TargetValue = 10;
        }
    }
}
