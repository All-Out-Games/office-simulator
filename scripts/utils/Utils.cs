using AO;

public static class Util
{
  private static readonly string[] graduations = { "Unused", "K", "M", "B", "T" };

  public static string FormatDouble(
      double value,
      int roundAtDigits = 4,
      int roundToDigits = 2,
      string separator = "",
      bool forceSign = false
  )
  {
    value = Math.Floor(value);
    string number = value.ToString("F0");

    int digitsLength = number.Length;
    if (value < 0)
    {
      digitsLength -= 1; // remove the `-` sign
    }
    if (digitsLength < roundAtDigits)
    {
      return number;
    }

    int graduationLevel = (digitsLength - 1);

    // Round graduation level to nearest step of 3.
    graduationLevel = graduationLevel - (graduationLevel % 3);

    // Graduation level is already a multiple of 3.
    int modified_level = graduationLevel / 3;

    // The string which will hold the numbers units.
    string graduation = "";

    if (modified_level < graduations.Length)
      graduation = graduations[modified_level];
    else
    {
      modified_level -= graduations.Length;

      graduation =
          modified_level < 26
              ? (new string((char)('A' + modified_level), 2))
              : string.Format("e+{0}", graduationLevel);
    }

    //return graduation;

    int decimalPlacement = digitsLength - graduationLevel;

    roundToDigits = Math.Min(roundToDigits, digitsLength - decimalPlacement);

    string beforeDecimal = number.Substring(0, decimalPlacement);
    string afterDecimal = number.Substring(decimalPlacement, roundToDigits);

    if (afterDecimal.Length > 0)
      afterDecimal = "." + afterDecimal;

    string sign = forceSign && Math.Sign(value) == 1 ? "+" : "";

    return sign + beforeDecimal + afterDecimal + separator + graduation;
  }

  public static int Modulo(int a, int b)
  {
    return (Math.Abs(a * b) + a) % b;
  }

  public static float Lerp(float a, float b, float t)
  {
    return a + (b - a) * t;
  }

  public static float Clamp(float value, float min, float max)
  {
    if (value < min) return min;
    if (value > max) return max;
    return value;
  }

  public static float Jitter(float t, float freq = 16)
  {
    double numerator = Math.Sin(freq * 2.0 * Math.PI * t);
    double denominator = 1.0 + 2.0 * Math.PI * 10.0 * Math.Pow(t, 4);
    return (float)(numerator / denominator);
  }

  public static float Punch(float t)
  {
    double numerator = t;
    double denominator = Math.Pow(1.44467, 32.0 * t);
    return (float)(32.0 * (numerator / denominator));
  }

  public static Vector2 RandomPositionInBox(Vector2 min, Vector2 max, Random rng)
  {
    Vector2 pos;
    pos.X = Lerp(min.X, max.X, (float)rng.NextDouble());
    pos.Y = Lerp(min.Y, max.Y, (float)rng.NextDouble());
    return pos;
  }

  public static bool Has(this string str)
  {
    return !string.IsNullOrEmpty(str);
  }

  public static void Assert(bool condition, string message = default)
  {
    if (condition)
    {
      return;
    }

    if (message.Has())
    {
      throw new Exception($"ASSERTION FAILED: {message}");
    }
    else
    {
      throw new Exception("ASSERTION FAILED");
    }
  }

  public static float NextFloat(this Random rng, float lo = 0, float hi = 1)
  {
    var result = rng.NextSingle();
    result = lo + (hi - lo) * result;
    return result;
  }

  public static double NextDouble(this Random rng, double lo = 0, double hi = 1)
  {
    var result = rng.NextDouble();
    result = lo + (hi - lo) * result;
    return result;
  }

  public static T Pop<T>(this List<T> list)
  {
    T result = list[list.Count - 1];
    list.RemoveAt(list.Count - 1);
    return result;
  }

  public static void UnorderedRemoveAt<T>(this List<T> list, int index)
  {
    list[index] = list[list.Count - 1];
    list.RemoveAt(list.Count - 1);
  }

  public static T GetRandom<T>(this List<T> list)
  {
    Assert(list != null);
    Assert(list.Count != 0);
    var rng = new Random();
    return list[rng.Next(list.Count)];
  }

  public static T GetRandom<T>(this T[] array)
  {
    Assert(array != null);
    Assert(array.Length != 0);
    var rng = new Random();
    return array[rng.Next(array.Length)];
  }

  public static int GetRandomIndex<T>(this List<T> list)
  {
    Assert(list != null);
    Assert(list.Count != 0);
    var rng = new Random();
    return rng.Next(list.Count);
  }

  public static int GetRandomIndex<T>(this T[] array)
  {
    Assert(array != null);
    Assert(array.Length != 0);
    var rng = new Random();
    return rng.Next(array.Length);
  }

  public static T IndexClamped<T>(this T[] array, int index)
  {
    Assert(array != null);
    Assert(array.Length != 0);
    return array[Math.Clamp(index, 0, array.Length - 1)];
  }

  public static bool Timer(ref float acc, float time)
  {
    if (acc >= time)
    {
      acc -= time;
      return true;
    }
    return false;
  }

  public static bool Timer(ref double acc, double time)
  {
    if (acc >= time)
    {
      acc -= time;
      return true;
    }
    return false;
  }
}
