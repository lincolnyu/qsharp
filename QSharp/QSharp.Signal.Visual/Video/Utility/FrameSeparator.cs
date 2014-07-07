using System;
using System.IO;
using QSharp.Scheme.Utility;


namespace QSharp.Signal.Visual.Video.Utility
{
    public class FrameSeparator
    {
        string InputFileName
        {
            set
            {
                myInputFn = value;
            }

            get
            {
                return myInputFn;
            }
        }

        string OLFileName
        {
            set
            {
                myOLFn = value;
            }

            get
            {
                return myOLFn;
            }
        }

        string OAlignedFileName
        {
            set
            {
                myOAlignedFn = value;
            }

            get
            {
                return myOAlignedFn;
            }
        }

        public FrameSeparator()
        {
        }

        public void SetSC(byte[] sc, int nBits)
        {
            mySc = sc;
            myNScBits = nBits;
        }

        public void SetSC_Mpeg4()
        {
            mySc = new byte[3];
            mySc[0] = mySc[1] = 0; mySc[2] = 1;
            myNScBits = 24;
        }

        public void SetSC_H263()
        {
            mySc = new byte[3];
            mySc[0] = mySc[1] = 0; mySc[2] = 0x80;
            myNScBits = 17;
        }

        protected bool GuessTypeFromExt()
        {
            int iDotPos = myInputFn.LastIndexOf('.');
            string sExt = myInputFn.Substring(iDotPos + 1);
            if (sExt == "263")
            {
                SetSC_H263();
            }
            else if (sExt == "mpeg4")
            {
                SetSC_Mpeg4();
            }
            else
            {
                return false;
            }
            return true;
        }

        protected static bool BitwiseEqual(byte[] a, byte[] b, int nBits)
        {
            int nBytes = nBits / 8;
            for (int i = 0; i < nBytes; i++)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }
            int nRemainingBits = nBits % 8;
            if (nRemainingBits == 0)
            {
                return true;
            }

            int nShift = 8 - nRemainingBits;
            byte abits = (byte)(a[nBytes] >> nShift);
            byte bbits = (byte)(b[nBytes] >> nShift);
            return abits == bbits;
        }

        public void DoSeparation()
        {
            try
            {
                Stream input = File.OpenRead(myInputFn);
                StreamWriter ol = File.CreateText(myOLFn);

                Stream oAligned = null;

                if (mySc == null)
                {
                    bool bTypeSet = GuessTypeFromExt();
                    if (!bTypeSet)
                    {
                        Console.WriteLine("! Unable to infer the stream type from the file extension, taking it as MPEG-4.");
                        SetSC_Mpeg4();
                    }
                }

                int nWrOAligned = 0;
                byte[] zeros = new byte[4]{0, 0, 0, 0};
                if (myOAlignedFn != null && myOAlignedFn != "")
                {
                    oAligned = File.OpenWrite(myOAlignedFn);
                }

                FileInfo fiInput = new FileInfo(myInputFn);
                int nLenInput = (int)fiInput.Length;

                StreamProbe sp = new StreamProbe(input);
                int nCompBytes = (myNScBits + 7) / 8;
                byte[] buf = new byte[nCompBytes];

                int i;
                for (i = 0; i <= nLenInput - nCompBytes; )
                {
                    sp.Read(buf, i, nCompBytes);
                    if (BitwiseEqual(buf, mySc, myNScBits))
                    {
                        ol.WriteLine(i.ToString() + ", ");
                        i += nCompBytes;
                        if (oAligned != null)
                        {
                            int nUnaligned = nWrOAligned % 4;
                            if (nUnaligned != 0)
                            {
                                oAligned.Write(zeros, 0, 4-nUnaligned);
                                nWrOAligned += 4-nUnaligned;
                            }
                            oAligned.Write(buf, 0, nCompBytes);
                            nWrOAligned += nCompBytes;
                        }
                    }
                    else
                    {
                        i++;    // TODO: optimization needed, consider KMP?
                        if (oAligned != null)
                        {
                            oAligned.Write(buf, 0, 1);
                            nWrOAligned++;
                        }
                    }
                }

                ol.WriteLine(nLenInput.ToString());

                if (oAligned != null)
                {
                    sp.Read(buf, i, nLenInput - i);
                    oAligned.Write(buf, 0, nLenInput - i);
                }


                input.Close();
                ol.Close();
            }
            catch(Exception)
            {
                Console.WriteLine("! Error occurred during separation.");
            }
            finally
            {
            }
        }

        /*
         * <summary>
         * This sample program mark the frame borders of a media stream 
         * (in most cases, a video file) by outputting the offsets
         *  of these borders to a file.
         * The indicator of border (startcode) can be those 
         * specified by any of the following video standards that 
         * are supported or customized by the user:
         *   MPEG-4 / H.264  0x 00 00 01
         *   H.263           
         * </summary>
         */
        public static void SampleMain(string[] args)
        {
            string fnIn = "in.dat";     // default input file
            string fnOL = "ol.txt";     // default offset list file
            string fnOAligned = "";     // default output frame-aligned file

            FrameSeparator vs = new FrameSeparator();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-i")
                {
                    i++;
                    fnIn = args[i];
                }
                else if (args[i] == "-ol")
                {
                    i++;
                    fnOL = args[i];
                }
                else if (args[i] == "-o_aligned")
                {   /* output frame-aligned file, 
                     * notice: offset list file is according to the original file 
                     */
                    i++;
                    fnOAligned = args[i];
                }
                else if (args[i] == "-t")
                {
                    i++;
                    if (args[i] == "mpeg4" || args[i] == "h264")
                    {
                        vs.SetSC_Mpeg4();
                    }
                    else if (args[i] == "h263")
                    {
                        vs.SetSC_H263();
                    }
                    else
                    {
                        Console.WriteLine("! Customized startcode currently not supported.");
                        return;
                    }
                }
                else if (args[i] == "--help")
                {   // print help frame
                    Console.WriteLine("Frame Separator -- Video Toolkit");
                    Console.WriteLine("framesep [options]");
                    Console.WriteLine("   -i <file>     Specifies input file ('in.dat' by default)");
                    Console.WriteLine("   -ol <file>    Specifies output offset list file ('segs.txt' by default)");
                    Console.WriteLine("   -t <type>     Specifies video stream type: any one of 'mpeg4', 'h264', 'h263'"
                                    + "                 ('mpeg4' by default)");
                    Console.WriteLine("   --help        Show this help");
                    return;
                }
                else
                {
                    Console.WriteLine("! Bad arguments.");
                    return;
                }
            }
            vs.InputFileName = fnIn;
            vs.OLFileName = fnOL;
            vs.OAlignedFileName = fnOAligned;

            vs.DoSeparation();
        }

    /* private data members */

        protected byte[] mySc = null;
        protected int myNScBits = 0;

        protected string myInputFn = "";
        protected string myOLFn = "";
        protected string myOAlignedFn = "";
    }
}
