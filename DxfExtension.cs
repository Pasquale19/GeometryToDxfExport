using netDxf;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace GeometryToDxfExport
{
   public static class DxfExtension
    {
        public static netDxf.Vector2 ToVector2(this System.Windows.Point ell)
        {
            Vector2 P = new Vector2(ell.X, ell.Y);
            return P;
        }
    }
}
