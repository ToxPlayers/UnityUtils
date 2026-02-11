using System;
using System.Collections.Generic; 
using System.Runtime.CompilerServices; 
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;

static public class CExtensions
{
	const int INLINE = (int)MethodImplOptions.AggressiveInlining;

	#region BitFlags

	[MethodImpl(INLINE)]
	static public uint UncheckedUINT(this int v) => unchecked((uint)v); 
    [MethodImpl(INLINE)]
    public static int UncheckedINT(this uint v) => unchecked((int)v);
    [MethodImpl(INLINE)] 
    public static uint WangHash(this uint n)
    {
        // https://gist.github.com/badboy/6267743#hash-function-construction-principles
        // Wang hash: this has the property that none of the outputs will
        // collide with each other, which is important for the purposes of
        // seeding a random number generator.  This was verified empirically
        // by checking all 2^32 uints.
        n = (n ^ 61u) ^ (n >> 16);
        n *= 9u;
        n = n ^ (n >> 4);
        n *= 0x27d4eb2du;
        n = n ^ (n >> 15);

        return n;
    } 
	[MethodImpl(INLINE)]
	public static int BitsSetCount(this int mask)
	{
        mask = mask - ((mask >> 1) & 0x55555555); // reuse input as temporary
        mask = (mask & 0x33333333) + ((mask >> 2) & 0x33333333); // temp
        return ((mask + (mask >> 4) & 0xF0F0F0F) * 0x1010101) >> 24; // count
    }
    [MethodImpl(INLINE)]
	public static int ToFlagsMask(this ref int bitIndex) => 1 << bitIndex;
    [MethodImpl(INLINE)]
    public static bool IsSetBitFlags(this int flagMask, int flags) 
    { 
        return (flagMask & flags) != 0;
    }
    [MethodImpl(INLINE)]
    public static void SetBitFlags(this ref int flagMask, int flags)
    {  
        flagMask |= flags;
    }
    [MethodImpl(INLINE)]
    public static void UnsetBitFlags(this ref int flagMask, int flags) 
    {  
        flagMask &= (~flags);
    }
	static public IEnumerable<int> EnumFlags(this int mask)
	{
		for (int i = 0; i < 32; ++i)
		{
		  var toFlag = 1 << i;
		  if (toFlag.IsSetBitFlags(mask))
			  yield return toFlag;
		}
	}
	#endregion

	[MethodImpl(INLINE)]
	static public float GetFractional(this in float f)
	{
		var floored = MathF.Floor(f);
		return f - floored;
    }

	static public T WrapIndex<T>(this T[] arr, int idx) => arr[MathU.WrapIndex(idx, arr.Length)];
	static public T WrapIndex<T>(this IList<T> lst, int idx) => lst[MathU.WrapIndex(idx, lst.Count)];
    static public void SetLength<T>(this IList<T> lst, int length)
	{
		if (lst == null)
			throw new ArgumentNullException("list");

		if (length < 0)
			throw new ArgumentException("Length must be larger than or equal to 0.");

		if (lst.GetType().IsArray)
			throw new ArgumentException("Cannot use the SetLength extension method on an array. Use Array.Resize or the ListUtilities.SetLength(ref IList<T> list, int length) overload.");

		while (lst.Count < length)
			lst.Add(default);

		while (lst.Count > length)
			lst.RemoveAt(lst.Count - 1);
	}
	
    [MethodImpl(INLINE)] static public string TrimEndUntil(this string input, in char until) => input.Substring(input.LastIndexOf(until) + 1); 
	[MethodImpl(INLINE)] static public string TrimEndUntil(this string input, in string until) => input.Substring(input.LastIndexOf(until) + 1);
	[MethodImpl(INLINE)] static public string TrimStartUntil(this string input , in char until) => input.Substring(input.IndexOf(until) + 1); 
	[MethodImpl(INLINE)] static public string TrimStartUntil(this string input , in string until) => input.Substring(input.IndexOf(until) + 1); 

	[MethodImpl(INLINE)] 	static public string ToStringEnum(this IEnumerable enumerable)
	{
		var str = "";
		var i = 0;
		foreach (var e in enumerable)
			str += $"[{i++}]: {e}\n";
		return str;
	}
    [MethodImpl(INLINE)]
    static public string ToStringEnum(this IEnumerable enumerable, string preStr, string postStr)
    {
        var str = ""; 
        foreach (var e in enumerable)
            str += $"{preStr}{e}{postStr}";
		str.TrimStart(preStr);
		str.TrimEnd(postStr);
        return str;
    }

    [MethodImpl(INLINE)]
	static public bool IsNullOrEmpty(this ICollection col) => col is null || col.Count == 0;	

	[MethodImpl(INLINE)]
	static public void SafeSub<T>(this Action subTo , Action subscribedAction )
	{
		subTo -= subscribedAction;
		subTo += subscribedAction;
	}

	[MethodImpl(INLINE)]
	static public T Peek<T>(this IList<T> list) => list[^1];
	[MethodImpl(INLINE)]
	static public string ToStringForEach<T>(this ICollection<T> coll ,string preString = "[{i++}]: ", string postString = "\n")
	{
		if (coll == null)
			return "null collection";
		var str = "";
		var i = 0;
		var count = coll.Count;
        foreach (var item in coll)
		{
			i++; 
			str += $"{preString}{item}{(i == count ? "" : postString)}"; 
        }
        return str;
	} 

    [MethodImpl(INLINE)]
	static public bool ValidIndex<T>(this ICollection<T> collection , int index)
	{
		return index >= 0 && index < collection.Count;
	}
	[MethodImpl(INLINE)]
	static public bool TryCast<T, TCast>(this T obj , out TCast castedObj) where TCast : class
	{
		castedObj = obj as TCast; 
		return castedObj != null;
	}
	[MethodImpl(INLINE)]
	static public bool IsEmpty<T>(this ICollection<T> col) => col.Count <= 0;
	 
	public static bool IsSubclassOf(Type type, Type baseType)
	{
		if (type == null || baseType == null || type == baseType)
			return false;

		if (baseType.IsGenericType == false)
		{
			if (type.IsGenericType == false)
				return type.IsSubclassOf(baseType);
		}
		else
		{
			baseType = baseType.GetGenericTypeDefinition();
		}

		type = type.BaseType;
		Type objectType = typeof(object);
		while (type != objectType && type != null)
		{
			Type curentType = type.IsGenericType ?
				type.GetGenericTypeDefinition() : type;
			if (curentType == baseType)
				return true;

			type = type.BaseType;
		}

		return false;
	}
	[MethodImpl(INLINE)]
    static public int RollIndex(this int index, int length)
	{
		if (index >= length)
			index = 0;
		else if (index < 0)
			index = length - 1;
		return index;
	}
	[MethodImpl(INLINE)]
    static public T RollIndex<T>(this IList<T> arr, int index) => arr[RollIndex(index, arr.Count)];
	[MethodImpl(INLINE)] public static List<T> ToList<T>(this IEnumerable<T> enumerator) => new List<T>(enumerator); 
	[MethodImpl(INLINE)] static public IEnumerable<Type> GetDerivingTypes(this Type baseType , bool exludeAbstract = true)
	{ 
		foreach( var assem in AppDomain.CurrentDomain.GetAssemblies() )
		{
			foreach ( var t in assem.GetTypes() )
			{
				if ( IsSubclassOf(t,baseType) )
				{
					if (exludeAbstract && t.IsAbstract)
						continue;
					yield return t;
				}
			}
		} 
	}
	[MethodImpl(INLINE)]
	static public string TrimEnd(this string source, string trimValue)
	{
		if (!source.EndsWith(trimValue))
			return source;

		return source[trimValue.Length..]; 
	}
    [MethodImpl(INLINE)]
    static public string TrimStart(this string source, string trimValue)
    { 
        if (!source.StartsWith(trimValue))
            return source;
		 
        return source[..trimValue.Length];
    }

    [MethodImpl(INLINE)]	
	static public void LoopRemove<T>(this LinkedList<T> list, Predicate<T> shouldRemove, long maxLoopTimeMilliseconds)
	{
		var watch = System.Diagnostics.Stopwatch.StartNew();
		var node = list.First; 
        while (node != null &&  watch.ElapsedMilliseconds < maxLoopTimeMilliseconds )
		{ 
			var nxtNode = node.Next;
			if(shouldRemove(node.Value))
				list.Remove(node);
			node = nxtNode;
        }
	}

	static public void SafeSingleHook(this Action action, Action onAction)
	{
		action -= onAction;
		action += onAction;
	}


    [MethodImpl(INLINE)]
    static public void LoopRemove<T>(this LinkedList<T> list, Action<T> actionOnItem, float maxTime = -1f)
    {
        var watch = System.Diagnostics.Stopwatch.StartNew();
        var shouldCheckTime = maxTime >= 0;
        var node = list.First;
        while (node != null && (!shouldCheckTime || watch.ElapsedMilliseconds < maxTime))
        {
            var nxtNode = node.Next;
			actionOnItem(node.Value);
			list.Remove(node);
            node = nxtNode;
        }
    }
    static public int EnumCount<T>() => EnumCount(typeof(T));  
    static public int EnumCount(Type enumType)
    {
        return Enum.GetValues(enumType).Length;
    }
}
