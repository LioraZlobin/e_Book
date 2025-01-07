using iTextSharp.text;
using System;

namespace e_Book.Controllers
{
    internal class Document
    {
        private object a4;

        public Document(object a4)
        {
            this.a4 = a4;
        }

        internal void Open()
        {
            throw new NotImplementedException();
        }

        internal void Add(Paragraph paragraph)
        {
            throw new NotImplementedException();
        }

        internal void Close()
        {
            throw new NotImplementedException();
        }

        internal void Add(iText.Layout.Element.Paragraph paragraph)
        {
            throw new NotImplementedException();
        }
    }
}