using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace VideoHomeStorage.FE
{
    public class VHSEncoder
    {
        public enum BitDepth { bit = 1, nibble = 4, byt = 8 }; // byte is reserved\

        // Configuration constants
        private static int streamWidth = 320; // Horizontal resolution of frame
        private static int streamHeight = 240; // Vertical resolution of frame

        private BitDepth bitDepth = BitDepth.nibble; // Number of bits per symbol
        // Block size is 8 symbols
        private int hBlocks = 2; // Number of data blocks per line
        private bool parity = true; // Whether a pairity symbol is included after each data block
        private int vRows = 1; // Number of rows/lines per frame

        // Derived constants
        private int numCols;
        private int numRows;
        private int symbolWidth;
        private int symbolHeight;
        private int bytesPerFrame;

        public int BytesPerFrame {
            get { return bytesPerFrame; }
        }

        /// <summary>
        /// Instantiate a VHSEncoder object. This will be used to hold codec information that stays the same across frames
        /// </summary>
        /// <param name="numBlocksPerRow">The number of 8 symbol (+ parity, if enabled) blocks in a row</param>
        /// <param name="numRowsPerFrame">The number of rows in a frame</param>
        /// <param name="bitsPerSymbol">The number of bits encoded per symbol</param>
        public VHSEncoder(int numBlocksPerRow = 2, int numRowsPerFrame = 1, BitDepth bitsPerSymbol = BitDepth.nibble, bool parity = true)
        {
            bitDepth = bitsPerSymbol;
            hBlocks = numRowsPerFrame;
            this.parity = parity;
            vRows = numBlocksPerRow;

            numCols = (hBlocks * 8) + (parity ? hBlocks : 0);
            numRows = vRows;
            symbolWidth = streamWidth / numCols;
            symbolHeight = streamHeight / numRows;
            bytesPerFrame = (int)(hBlocks * 8 * vRows * ((float)bitDepth / 8F));
        }

        /// <summary>
        /// Encode an array of bytes into an image "frame" using the settings initialized in the class
        /// </summary>
        /// <param name="data">The byte array to be encoded</param>
        /// <returns>The encoded bitmap image</returns>
        public Bitmap Encode(byte[] data)
        {
            // Input data / settings sanity check
            if (data.Count() > bytesPerFrame)
            {
                throw new ArgumentException("Given " + data.Count() + " bytes. Cannot encode more than " + bytesPerFrame + " bytes per frame!");
            }

            Bitmap bmp = new Bitmap(streamWidth, streamHeight);
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.FromArgb(127, 127, 127)); // Clear it to half grey so any unwritten pixels have less sharp edges
            // Note: there will be unwritten pixels at the right side of the screen unless things divide perfectly, because it's just
            //       so much easier computationally to place the symbols.

            int iData = 0; // Iterator through data [0:data.Count()-1]
            int iRow = 0;  // Iterator through rows [0:numRows-1]
            int iCol = 0;  // Iterator through columns [0:numCols-1]
            int iByte = 0; // Used for bit-depths less than byte to keep track of the position in the current bit

            int val = 0;   // The value being written to the current symbol

            while (iData < data.Count())
            {
                if(parity && ((iCol + 1) % 9 == 0))
                {
                    // This symbol is parity
                    val =  calculateParity(data, iData);
                    fillSymbol(g, iRow, iCol, val);
                }
                else
                {
                    // This symbol is data
                    val = calculateValue(data, iData, iByte, bitDepth);
                    fillSymbol(g, iRow, iCol, val);

                    // Data position logic
                    iByte++;
                    if (iByte >= (8 / (int)bitDepth))
                    {
                        iByte = 0;
                        iData++;
                    }
                }

                // Frame position logic
                iCol++;
                if(iCol >= numCols)
                {
                    iCol = 0;
                    iRow++;
                }

                // Frame/Data position sanity check
                if(iRow >= numRows && iData < data.Count())
                {
                    throw new ApplicationException("Frame position and Data position out of sync! Frame encode failed!");
                }
            }

            return bmp;
        }

        private int calculateValue(byte[] data, int i_data, int bytePos, BitDepth bitDepth)
        {
            int val;
            switch(bitDepth)
            {
                case BitDepth.bit:
                    BitArray ba = new BitArray(new byte[] { data[i_data] });
                    val = ba[bytePos] ? 1 : 0;
                    return val * 255;
                case BitDepth.nibble:
                    val = (int)data[i_data];
                    if (bytePos == 0)
                    {
                        val = val >> 4;
                    }
                    else // bytePos == 1
                    {
                        val &= 0x0F;
                    }
                    return val * 17;
                case BitDepth.byt:
                    val = (int)data[i_data];
                    return val;
                default:
                    throw new ApplicationException("Invalid bit depth! This shouldn't happen...");
            }
        }

        private int calculateParity(byte[] data, int i_data)
        {
            
            switch (bitDepth)
            {

                case BitDepth.bit:
                    BitArray ba = new BitArray(new byte[] { data[i_data] });
                    bool bval = ba[0];
                    for (int i_bit = 1; i_bit <= 7; i_bit++)
                        bval = bval ^ ba[i_bit];
                    return Convert.ToInt16(bval) * 255;

                case BitDepth.nibble:
                    int p_data = i_data - 3;
                    BitArray ret_val = new BitArray(8);
                    for (int i_bit = 0; i_bit < 4; i_bit++)
                    {
                        ba = new BitArray(new byte[] { data[p_data + i_bit] });
                        bval = ba[0];
                        for (int j_bit = 1; j_bit <= 7; j_bit++)
                            bval = bval ^ ba[j_bit];
                        ret_val[i_bit] = bval;
                    }
                    int[] ret = new int[1];
                    ret_val.CopyTo(ret, 0);
                    return ret[0] * 17;

                case BitDepth.byt:
                    p_data = i_data - 7;
                    ret_val = new BitArray(8);
                    for (int i_bit = 0; i_bit < 8; i_bit++)
                    {
                        ba = new BitArray(new byte[] { data[p_data + i_bit] });
                        bval = ba[0];
                        for (int j_bit = 1; j_bit <= 7; j_bit++)
                            bval = bval ^ ba[j_bit];
                        ret_val[i_bit] = bval;
                    }
                    ret = new int[1];
                    ret_val.CopyTo(ret, 0);
                    return ret[0];
                default:
                    throw new ApplicationException("Invalid bit depth! This shouldn't happen...");
            }
        }

        private void fillSymbol(Graphics g, int i_row, int i_col, int val)
        {
            int xPos = i_col * symbolWidth;
            int yPos = i_row * symbolHeight;
            Rectangle symbol = new Rectangle(xPos, yPos, symbolWidth, symbolHeight);
            Color valColor = Color.FromArgb(val, val, val);
            SolidBrush b = new SolidBrush(valColor);
            g.FillRectangle(b, symbol);
        }

        public void Decode()
        {

        }
    }
}
