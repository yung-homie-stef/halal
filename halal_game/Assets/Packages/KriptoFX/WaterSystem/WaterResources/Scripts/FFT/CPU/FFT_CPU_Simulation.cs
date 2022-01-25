using System.Threading;
using UnityEngine;

public class FFT_CPU_Simulation
{
    Vector2[] input_H;
    Vector2[] complex_H;
    Vector2[] intermediates_H;
    Vector2[] scratch_H;
    Vector2[] inpButt;

    bool firstThreadDone;
    bool secondThreadDone;

    int passes;
    int size;
    ButterflyHashProperties[,] hashFunction;
    FFT_CPU.OutputFFTData currentBuffer;

    public FFT_CPU_Simulation(int Size, FFT_CPU.OutputFFTData outputFftData)
    {
        currentBuffer = outputFftData;
        Initialize(Size);
    }

    private void Initialize(int Size)
    {
        size = Size;
        passes = Mathf.RoundToInt(Mathf.Log(size, 2));

        intermediates_H = new Vector2[size];
        scratch_H = new Vector2[size];
        complex_H = new Vector2[size*size];

        hashFunction = new ButterflyHashProperties[size, passes];
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < passes; j++)
            {
                hashFunction[i, j] = ButteflyHashFunction(i, j);
            }
        }

        if (currentBuffer.OutputPixels == null || currentBuffer.OutputPixels.Length != size * size)
            currentBuffer.OutputPixels = new Color[size*size];
    }

    public void Compute(Vector2[] inpHeight, Vector2[] inpButterfly, int colorChannel, int currentSize)
    {
        if (currentSize != size)
        {
            Initialize(currentSize);
        }
        input_H = inpHeight;
        inpButt = inpButterfly;
        FFT_H();
        FFT_V(colorChannel);
    }
    
    void FFT_H()
    {
        bool pingpong = (passes % 2 == 0);
        int index;
        ButterflyHashProperties hash;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; ++x)
            {
                index = y * size + x;
                intermediates_H[x].x = input_H[index].x;
                intermediates_H[x].y = -input_H[index].y;
            }

            for (int passIndex = 0; passIndex < passes; ++passIndex)
            {
                for (int x = 0; x < size; ++x)
                {
                    hash = hashFunction[x, passIndex];
                    if (hash.pingPong)
                    {
                        scratch_H[x].x = inpButt[hash.index].x * intermediates_H[hash.y].x - inpButt[hash.index].y * intermediates_H[hash.y].y + intermediates_H[hash.x].x;
                        scratch_H[x].y = inpButt[hash.index].y * intermediates_H[hash.y].x + inpButt[hash.index].x * intermediates_H[hash.y].y + intermediates_H[hash.x].y;
                    }
                    else
                    {
                        intermediates_H[x].x = inpButt[hash.index].x * scratch_H[hash.y].x - inpButt[hash.index].y * scratch_H[hash.y].y + scratch_H[hash.x].x;
                        intermediates_H[x].y = inpButt[hash.index].y * scratch_H[hash.y].x + inpButt[hash.index].x * scratch_H[hash.y].y + scratch_H[hash.x].y;
                    }
                }
            }
            for (int x = 0; x < size; ++x)
            {
                index = y * size + x;
                if (pingpong)
                {

                    complex_H[index].x = intermediates_H[x].x;
                    complex_H[index].y = intermediates_H[x].y;
                }
                else
                {
                    complex_H[index].x = scratch_H[x].x;
                    complex_H[index].y = scratch_H[x].y;
                }
            }
        }
    }

    void FFT_V(int colorChannel)
    {
        bool pingpong = (passes % 2) == 0;

        ButterflyHashProperties hash;
        for (int x = 0; x < size; ++x)
        {
            for (int y = 0; y < size; ++y)
            {
                intermediates_H[y] = complex_H[y * size + x];
            }

            for (int passIndex = 0; passIndex < passes; ++passIndex)
            {
                for (int y = 0; y < size; ++y)
                {
                    hash = hashFunction[y, passIndex];
                    if (hash.pingPong)
                    {
                        scratch_H[y].x = inpButt[hash.index].x * intermediates_H[hash.y].x - inpButt[hash.index].y * intermediates_H[hash.y].y + intermediates_H[hash.x].x;
                        scratch_H[y].y = inpButt[hash.index].y * intermediates_H[hash.y].x + inpButt[hash.index].x * intermediates_H[hash.y].y + intermediates_H[hash.x].y;
                    }
                    else
                    {
                        intermediates_H[y].x = inpButt[hash.index].x * scratch_H[hash.y].x - inpButt[hash.index].y * scratch_H[hash.y].y + scratch_H[hash.x].x;
                        intermediates_H[y].y = inpButt[hash.index].y * scratch_H[hash.y].x + inpButt[hash.index].x * scratch_H[hash.y].y + scratch_H[hash.x].y;
                    }
                }
            }
            for (int y = 0; y < size; ++y)
            {
                var result = pingpong ? intermediates_H[y] : scratch_H[y];
                float sign = ((x + y) % 2) == 1 ? -1.0f : 1.0f;
                switch (colorChannel)
                {
                    case 0:
                        currentBuffer.OutputPixels[y * size + x].r = sign* result.x;
                        break;
                    case 1:
                        currentBuffer.OutputPixels[y * size + x].g = sign * result.x;
                        break;
                    case 2:
                        currentBuffer.OutputPixels[y * size + x].b = sign * result.x;
                        break;
                }
            }
        }
    }

    ButterflyHashProperties ButteflyHashFunction(int coord, int passIndex)
    {
        int indexA, indexB;
        int offset = 1 << passIndex;
        if ((coord / offset) % 2 == 1)
        {
            indexA = (coord - offset);
            indexB = coord;
        }
        else
        {
            indexA = coord;
            indexB = (coord + offset);
        }

        if (passIndex == 0)
        {
            indexA = (int)(Reverse(indexA) >> (32 - passes));
            indexB = (int)(Reverse(indexB) >> (32 - passes));
        }

        var pingpong = (passIndex % 2) == 0;
        var index = coord + passIndex * size;
        return new ButterflyHashProperties(indexA, indexB, pingpong, index);
    }

    struct ButterflyHashProperties
    {
        public int x;
        public int y;
        public bool pingPong;
        public int index;

        public ButterflyHashProperties(int X, int Y, bool Pingpong, int Index)
        {
            x = X;
            y = Y;
            pingPong = Pingpong;
            index = Index;
        }
    }

    uint Reverse(int x1)
    {
        uint x = (uint)x1;
        uint y = 0;
        for (int i = 0; i < 32; ++i)
        {
            y <<= 1;
            y |= (x & 1);
            x >>= 1;
        }
        return y;
    }
}



