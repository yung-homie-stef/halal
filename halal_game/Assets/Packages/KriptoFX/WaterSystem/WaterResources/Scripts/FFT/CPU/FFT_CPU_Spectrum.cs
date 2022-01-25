using UnityEngine;

public class FFT_CPU_Spectrum
{
    UpdateSpectrumHashProperties[,] hashFunc;
    float[] sinHash;
    float[] cosHash;
    Vector4[] initColors;

    public Vector2[] ResultHeight;
    public Vector2[] ResultDisplaceX;
    public Vector2[] ResultDisplaceZ;
   
    float INVPI2 = 0.63661977236f;
    float HPI = 1.57079632679f;
    float PI2 = 6.28318530717f;
    float PI4	= 0.33661977236f;
    int rngState;
    private const float Gravity = 9.81f;
    int size;

    UpdateSpectrumHashProperties hash;
    float scaledTime;
    int index;
    float hX;
    float hY;
    float sw;
    float cw;

    public FFT_CPU_Spectrum(int Size)
    {
        Initialize(Size);
    }

    private void Initialize(int Size)
    {
        size = Size;

        sinHash = new float[6284]; //3.1415 * 2 * 1000
        cosHash = new float[6284];
        for (int i = 0; i < 6284; i++)
        {
            sinHash[i] = Mathf.Sin(i/100f);
            cosHash[i] = Mathf.Cos(i/100f);
        }
        initColors = new Vector4[size*size];
        ResultHeight = new Vector2[size*size];
        ResultDisplaceX = new Vector2[size*size];
        ResultDisplaceZ = new Vector2[size*size];
    }

    public void InitializeSpectrum(float domainSize, float windSpeed, float direction, int currentSize)
    {
        if (size != currentSize)
        {
            Initialize(currentSize);
        }
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                //EncinoSpectrumInit(j, i, domainSize, windSpeed, direction);
                int center = (size/2);
                int coordX = i - center;
                int coordY = j - center;
                if (coordX != 0 || coordY != 0)
                {
                    float k_x = PI2*coordX/domainSize;
                    float k_y = PI2*coordY/domainSize;
                    float kMag = new Vector2(k_x, k_y).magnitude;
                    rngState = WangHash(j*size + i);

                    var w = Mathf.Sqrt(Mathf.Abs(Gravity*kMag));
                    var dwdk = Gravity/(2.0f*w);

                    float spectrum = PiersonMoskowitzSpectrum(w, windSpeed);
                    float deltaSPos = spectrum;
                    float deltaSNeg = spectrum;

                    float dK = PI2/domainSize;
                    float thetaPos = Mathf.Atan2(-k_y, k_x);
                    float thetaNeg = Mathf.Atan2(k_y, -k_x);
                    deltaSPos *= PosCosSquaredDirectionalSpreading(thetaPos, direction);
                    deltaSNeg *= PosCosSquaredDirectionalSpreading(thetaNeg, direction);
                    deltaSPos *= (dK*dK)*dwdk/kMag;
                    deltaSNeg *= (dK*dK)*dwdk/kMag;

                    float ampPos = RandGauss()*Mathf.Sqrt(Mathf.Abs(deltaSPos)*2.0f);
                    float ampNeg = RandGauss()*Mathf.Sqrt(Mathf.Abs(deltaSNeg)*2.0f);

                    float phasePos = RandFloat()*PI2;
                    float phaseNeg = RandFloat()*PI2;

                    float colX = ampPos*Mathf.Cos(phasePos);
                    float colY = ampPos*-Mathf.Sin(phasePos);
                    float colZ = ampNeg*Mathf.Cos(phaseNeg);
                    float colA = ampNeg*-Mathf.Sin(phaseNeg);
                    initColors[j*size + i] = new Vector4(colX, colY, colZ, colA);
                }
            }
        }
        hashFunc = new UpdateSpectrumHashProperties[size, size];
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                hashFunc[i, j] = UpdateHashFunc(i, j, domainSize);
            }
        }
    }

    public void GetUpdatedSpectrum(float time, int currentSize)
    {
        //if (currentSize != size)
        //{
        //    Initialize(currentSize);
        //}

        scaledTime = time/10;

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                hash = hashFunc[i, j];
                index = (int)((hash.w * scaledTime % PI2) * 1000);
                sw = sinHash[index];
                cw = cosHash[index];
                index = hash.index;
                hX = initColors[index].x * cw - initColors[index].y * -sw + initColors[index].z * cw - initColors[index].w * sw;
                hY = initColors[index].x * -sw + initColors[index].y * cw + initColors[index].z * sw + initColors[index].w * cw;
                ResultHeight[index].x = hX;
                ResultHeight[index].y = hY;

                ResultDisplaceX[index].x = -hY * hash.k_x;
                ResultDisplaceX[index].y = hX * hash.k_x;

                ResultDisplaceZ[index].x = -hY * hash.k_y;
                ResultDisplaceZ[index].y = hX * hash.k_y;
            }
        }
    }

    int WangHash(int seed)
    {
        seed = (seed ^ 61) ^ (seed >> 16);
        seed *= 9;
        seed = seed ^ (seed >> 4);
        seed *= 0x27d4eb2d;
        seed = seed ^ (seed >> 15);
        return seed;
    }

    int Rand()
    {
        rngState ^= (rngState << 13);
        rngState ^= (rngState >> 17);
        rngState ^= (rngState << 5);
        return Mathf.Abs(rngState);
    }

    float RandFloat()
    {
        return Rand() / 4294967296.0f;
    }

    float RandGauss()
    {
        float u1 = RandFloat();
        float u2 = RandFloat();
        if (u1 < 1e-6f)
            u1 = 1e-6f;
        return Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Cos(PI2 * u2);
    }

    //float PhillipsSpectrum(float w, float windSpeed)
    //{
    //    float A = 0.01f;
    //    float B = Gravity / windSpeed;
    //    return AlphaBetaSpectrum(A, B, Gravity, w, 1.0f);
    //}

    float PiersonMoskowitzSpectrum(float w, float windSpeed)
    {
        float wm = 0.87f * Gravity / windSpeed;
        return AlphaBetaSpectrum(8.1e-3f, 1.291f, Gravity, w, wm);
    }

    float AlphaBetaSpectrum(float A, float B, float g, float w, float wm)
    {
        return (A * g * g / Mathf.Pow(w, 5.0f)) * Mathf.Exp(-B * Mathf.Pow(wm / w, 4.0f));
    }

    float PosCosSquaredDirectionalSpreading(float theta, float direction)
    {
        if (theta > -HPI && theta < HPI)
        {
            float ct = Mathf.Cos(theta);
            return INVPI2 * (ct * ct) * (1 - direction) + PI4 * direction;
        }
        else
        {
            return PI4 * direction;
        }
    }

    struct UpdateSpectrumHashProperties
    {
        public float k_x;
        public float k_y;
        public float w;
        public int index;
    }

    UpdateSpectrumHashProperties UpdateHashFunc(int x, int y, float domainSize)
    {
        int center = (size / 2);
        int coordX = x - center;
        int coordY = y - center;

        float k_x = PI2 * coordX / domainSize;
        float k_y = PI2 * coordY / domainSize;
        float kMag = Mathf.Sqrt(k_x * k_x + k_y * k_y);

        float w = Mathf.Sqrt(Mathf.Abs(Gravity * kMag));

        kMag += 0.00001f;
        k_x /= kMag;
        k_y /= kMag;
        var index = x * size + y;
        return new UpdateSpectrumHashProperties {index = index, k_x = k_x, k_y = k_y, w = w};
    }
}
