namespace Matrices.Matrix;

public class SparseMatrix : Matrix
{
    private Dictionary<(int, int), float> matrix;
    public override float this[int row, int col]
    {
        get
        {
            if  (row < 0 || col < 0 || row >= Rows || col >= Columns)
                throw new ArgumentException("Index out of range.");
            return matrix.GetValueOrDefault((row, col), 0f);
        }
        set
        {
            if  (row < 0 || col < 0 || row >= Rows || col >= Columns)
                throw new ArgumentException("Index out of range.");
            if (value == 0f)
            {
                matrix.Remove((row, col));
            }
            matrix[(row, col)] = value;
        }
    }
    
    public SparseMatrix(int rows, int cols): base(rows, cols)
    {
        matrix = new Dictionary<(int, int), float>();
    }
    
    public override void Identity()
    {
        if (Rows != Columns)
        {
            throw new InvalidOperationException("Matrix is not a square matrix");
            
        }

        matrix.Clear();

        for (int i = 0; i < Rows; i++)
            matrix[(i, i)] = 1.0f;
        
    }

    public override Matrix Transpose()
    {
        SparseMatrix result = new SparseMatrix(Columns, Rows);
        foreach (var item in matrix)
        {
            (int row, int col) = item.Key;
            result.matrix[(col, row)] = item.Value;
        }
        
        return result;
    }

    public override float Norm()
    {
        float result = 0;
        foreach (float val in matrix.Values)
            result += val * val;
        
        return MathF.Sqrt(result);
    }
    
    protected override Matrix GetInstance(int row, int col)
    {
        return new SparseMatrix(row, col);
    }
    
}