﻿using Eto.Drawing;
using Eto.Forms;
using OpenSage.Data;
using OpenSage.Data.Tga;
using OpenSage.DataViewer.Framework;

namespace OpenSage.DataViewer.UI.Viewers
{
    public sealed class TgaView : ImageView
    {
        public TgaView(IFileSystemEntry entry)
        {
            var tgaFile = TgaFile.FromFileSystemEntry(entry);

            var data = TgaFile.ConvertPixelsToRgba8(tgaFile);

            var bmpData = PngUtility.ConvertToPng(
                data,
                tgaFile.Header.Width,
                tgaFile.Header.Height);

            Style = "nearest-neighbor";
            Image = new Bitmap(bmpData);
        }
    }
}
