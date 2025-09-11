using System;
using System.Windows.Forms;

namespace MyServer_001
{
    public static class Extensions
    {
        public static void SafeInvoke(this Control control, Action action)
        {
            if (control.InvokeRequired)
                control.Invoke(action);
            else
                action();
        }

    }
}
