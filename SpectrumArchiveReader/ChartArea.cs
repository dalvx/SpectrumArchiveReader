using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SpectrumArchiveReader
{
    public class ChartArea : UserControl
    {
        private bool disposed = false;
        private IntPtr bitmapGDI;
        private IntPtr oldBitmap;
        public IntPtr bDC;

        new public Size Size
        {
            get { return base.Size; }
            set
            {
                if (value == base.Size) return;
                base.Size = value;
                WinApi.SelectObject(bDC, oldBitmap);
                IntPtr hDC = WinApi.GetDC(Handle);
                IntPtr newBitmap = WinApi.CreateCompatibleBitmap(hDC, value.Width, value.Height);
                WinApi.ReleaseDC(Handle, hDC);
                WinApi.SelectObject(bDC, newBitmap);
                WinApi.DeleteObject(bitmapGDI);
                bitmapGDI = newBitmap;
            }
        }

        public event KeyPressSpecialEventHandler KeyPressSpecial;

        public delegate void KeyPressSpecialEventHandler(object sender, Keys keyData);

        public ChartArea(Control parent)
        {
            Parent = parent;
            Left = 0;
            Top = 0;
            base.Height = 10;
            base.Width = 10;

            IntPtr hDC = WinApi.GetDC(Handle);
            bDC = WinApi.CreateCompatibleDC(hDC);
            bitmapGDI = WinApi.CreateCompatibleBitmap(hDC, Height, Width);
            oldBitmap = WinApi.SelectObject(bDC, bitmapGDI);
            WinApi.ReleaseDC(Handle, hDC);
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if ((keyData >= Keys.F1 && keyData <= Keys.F24)
                || keyData == Keys.Escape
                || keyData == Keys.Up
                || keyData == Keys.Down
                || keyData == Keys.Delete
                || keyData == Keys.Home
                || keyData == Keys.End)
            {
                if (KeyPressSpecial != null) KeyPressSpecial(this, keyData);
                return true;
            }
            return base.ProcessDialogKey(keyData);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            IntPtr DC = e.Graphics.GetHdc();
            Rectangle r = e.ClipRectangle;
            WinApi.BitBlt(DC, r.X, r.Y, r.Right, r.Bottom, bDC, r.X, r.Y, WinApi.SRCCOPY);
            e.Graphics.ReleaseHdc();
        }

        public void Repaint()
        {
            IntPtr DC = WinApi.GetDC(Handle);
            WinApi.BitBlt(DC, 0, 0, Width, Height, bDC, 0, 0, WinApi.SRCCOPY);
            WinApi.ReleaseDC(this.Handle, DC);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WinApi.WM_ERASEBKGND) return;
            base.WndProc(ref m);
        }

        // Метод закомментирован, т.к. реализован в родительском классе. Из него уже вызывается виртуальный метод Dispose(bool) этого класса.
        //// Implement IDisposable.
        //// Do not make this method virtual.
        //// A derived class should not be able to override this method.
        //public void Dispose()
        //{
        //    Dispose(true);
        //    // This object will be cleaned up by the Dispose method.
        //    // Therefore, you should call GC.SupressFinalize to
        //    // take this object off the finalization queue
        //    // and prevent finalization code for this object
        //    // from executing a second time.
        //    GC.SuppressFinalize(this);
        //}

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        protected override void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                // If disposing is false,
                // only the following code is executed.
                WinApi.SelectObject(bDC, oldBitmap);
                WinApi.DeleteDC(bDC);
                WinApi.DeleteObject(bitmapGDI);

                base.Dispose(disposing);

                // Note disposing has been done.
                disposed = true;
            }
        }

        // Use C# destructor syntax for finalization code.
        // This destructor will run only if the Dispose method
        // does not get called.
        // It gives your base class the opportunity to finalize.
        // Do not provide destructors in types derived from this class.
        ~ChartArea()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ResumeLayout(false);
        }
    }
}
