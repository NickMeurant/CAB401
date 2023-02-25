using System;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Printing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using FFTW.NET;
using NAudio;

namespace DigitalMusicAnalysis
{
    public class timefreq
    {
        public float[][] timeFreqData;
        public int wSamp;
        public Complex[] twiddles;

        public timefreq(float[] x, int windowSamp)
        {
            int ii;
            double pi = 3.14159265;
            Complex i = Complex.ImaginaryOne;
            this.wSamp = windowSamp;
            twiddles = new Complex[wSamp];
            for (ii = 0; ii < wSamp; ii++)
            {
                double a = 2 * pi * ii / (double)wSamp;
                twiddles[ii] = Complex.Pow(Complex.Exp(-i), (float)a);
            }

            timeFreqData = new float[wSamp / 2][];

            int nearest = (int)Math.Ceiling((double)x.Length / (double)wSamp);
            nearest = nearest * wSamp;

            Complex[] compX = new Complex[nearest];
            for (int kk = 0; kk < nearest; kk++)
            {
                if (kk < x.Length)
                {
                    compX[kk] = x[kk];
                }
                else
                {
                    compX[kk] = Complex.Zero;
                }
            }


            int cols = 2 * nearest / wSamp;

            for (int jj = 0; jj < wSamp / 2; jj++)
            {
                timeFreqData[jj] = new float[cols];
            }

            timeFreqData = stft(compX, wSamp);

        }

        float[][] stft(Complex[] x, int wSamp)
        {
            int ii = 0;
            int jj = 0;
            int kk = 0;
            int ll = 0;
            int N = x.Length;
            float fftMax = 0;

            float[][] Y = new float[wSamp / 2][];

            for (ll = 0; ll < wSamp / 2; ll++)
            {
                Y[ll] = new float[2 * (int)Math.Floor((double)N / (double)wSamp)];
            }
            
            float[] fftMaxArray = new float[(int)(2 * Math.Floor((double)N / (double)wSamp)) - 1]; // need to hold an array of fftMax's since now running in parallel

            Parallel.For(0, (int)(2 * Math.Floor((double)N / (double)wSamp)) - 1, new ParallelOptions { MaxDegreeOfParallelism = MainWindow.numThreads }, ii =>
            {

                Complex[] temp = new Complex[wSamp]; // each thread needs its "own" version of temp
                Complex[] tempFFT = new Complex[wSamp]; // each thread needs its "own" verion of tempFFT

                for (int jj = 0; jj < wSamp; jj++)
                {
                    temp[jj] = x[ii * (wSamp / 2) + jj];
                }

                tempFFT = fft(temp, tempFFT); // this calls the fftw function

                for (int kk = 0; kk < wSamp / 2; kk++)
                {
                    Y[kk][ii] = (float)Complex.Abs(tempFFT[kk]);

                    if (Y[kk][ii] > fftMaxArray[ii]) // fftMaxArray[ii] is the FFTMax that is associated with the current loop, every loop should have a unique ii value, so this wont break
                    {
                        fftMaxArray[ii] = Y[kk][ii];
                    }
                }
            });

            // find the max of the fftMaxArray and set that as fftmax

            fftMax = fftMaxArray.Max(value => value);

            for (ii = 0; ii < 2 * Math.Floor((double)N / (double)wSamp) - 1; ii++)
            {
                for (kk = 0; kk < wSamp / 2; kk++)
                {
                    Y[kk][ii] /= fftMax;
                }
            }

            DFT.Wisdom.Export("wisdom.txt"); // This allows the program to use optimised "plans" generated 

            return Y;
        }

        Complex[] fft(Complex[] temp, Complex[] tempFFT)  // uses FFTW
        {

            using (var pinIn = new PinnedArray<Complex>(temp))
            using (var pinOut = new PinnedArray<Complex>(tempFFT))
            {
                DFT.FFT(pinIn, pinOut); // computes the fft using the FFTW algorithm instead of the recursive algorithm present, DFT.FFT is a C# wrapper that calls DLL's that contain the FFTW code
            }

            return tempFFT;
        }

    }
}
