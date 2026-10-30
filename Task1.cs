namespace Task1;

public interface IAdder<in T> {
    public bool Add(T val);
}

public interface ITaker<out T> {
    public T? Take();
}

public class Box<T> {
    public T Value { get; set; }
    public Box(T val) { Value = val; }
    public override string ToString() { return $"Box: {Value}"; }
}

public class NumBox<T>: Box<T> where T : System.Numerics.INumber<T>
{
    public NumBox(T val) : base(val) { }

    public static NumBox<T> operator+(NumBox<T> a, NumBox<T> b)
    {
        return new NumBox<T>(a.Value+b.Value);
    }
}

public class StringBox: Box<String>
{
    public StringBox(string val) : base(val) { }

    public static StringBox operator+(StringBox a, StringBox b)
    {
        return new StringBox(a.Value+";"+b.Value);
    }
}

public class Shelf<T> : IAdder<T>, ITaker<T> {
    public int Space { get; init; } = 5;
    private int _current = 0;
    private T?[] _items;

    public Shelf(int space) {
        _items = new T?[space];
        Space = space;
    }

    public bool Add(T val)
    {
        if (_current >= Space)
        {
            return false;
        }
        else
        {
            _items[_current++] = val;
            return true;
        }
    }

    public T? Take()
    {
        if (_current > 0)
        {
            _current--;
            T? val = _items[_current];
            _items[_current] = default;
            return val;
        }
        else
        {
            return default;
        }
    }
}