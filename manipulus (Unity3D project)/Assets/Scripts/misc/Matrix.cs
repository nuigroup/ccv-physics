using UnityEngine;

using System.Collections;

public class Matrix
{
    public float[] m;

    public Matrix()
    {   
        LoadIdentity();
    }   

    // Loads this matrix with an identity matrix

    public void LoadIdentity()
    {
        m = new float[ 16 ];

        for( int x = 0 ; x < 16 ; ++x )
        {
            m[ x ] = 0;
        }   

        m [ 0 ] = 1;
        m [ 5 ] = 1;
        m [ 10 ] = 1;
        m [ 15 ] = 1;
    }

    // Returns a translation matrix along the XYZ axes

    public static Matrix Translate( float _X, float _Y, float _Z )
    {
        Matrix wk = new Matrix();

        wk.m [ 12 ] = _X;
        wk.m [ 13 ] = _Y;
        wk.m [ 14 ] = _Z;

        return wk;
    }

    // Returns a rotation matrix around the X axis

    public static Matrix RotateX( float _Degree )
    {
        Matrix wk = new Matrix();

        if( _Degree == 0 )
        {
            return wk;
        }

        float C = Mathf.Cos( _Degree * Mathf.Deg2Rad );
        float S = Mathf.Sin( _Degree * Mathf.Deg2Rad );

        wk.m [ 5 ] = C;
        wk.m [ 6 ] = S;
        wk.m [ 9 ] = -S;
        wk.m [ 10 ] = C;

        return wk;
    }

    // Returns a rotation matrix around the Y axis

    public static Matrix RotateY( float _Degree )
    {
        Matrix wk = new Matrix();

        if( _Degree == 0 )
        {
            return wk;
        }

        float C = Mathf.Cos( _Degree * Mathf.Deg2Rad );
        float S = Mathf.Sin( _Degree * Mathf.Deg2Rad );

        wk.m [ 0 ] = C;
        wk.m [ 2 ] = -S;
        wk.m [ 8 ] = S;
        wk.m [ 10 ] = C;

        return wk;
    }

    // Returns a rotation matrix around the Z axis

    public static Matrix RotateZ( float _Degree )
    {
        Matrix wk = new Matrix();

        if( _Degree == 0 )
        {
            return wk;
        }

        float C = Mathf.Cos( _Degree * Mathf.Deg2Rad );
        float S = Mathf.Sin( _Degree * Mathf.Deg2Rad );

        wk.m [ 0 ] = C;
        wk.m [ 1 ] = S;
        wk.m [ 4 ] = -S;
        wk.m [ 5 ] = C;

        return wk;
    }

    // Returns a scale matrix uniformly scaled on the XYZ axes

    public static Matrix Scale( float _In )
    {
        return Matrix.Scale3D( _In, _In, _In );
    }

    // Returns a scale matrix scaled on the XYZ axes

    public static Matrix Scale3D( float _X, float _Y, float _Z )
    {
        Matrix wk = new Matrix();

        wk.m [ 0 ] = _X;
        wk.m [ 5 ] = _Y;
        wk.m [ 10 ] = _Z;

        return wk;
    }

    // Transforms a vector with this matrix

    public Vector3 TransformVector( Vector3 _V )
    {
        Vector3 vtx = new Vector3(0,0,0);

        vtx.x = ( _V.x * m [ 0 ] ) + ( _V.y * m [ 4 ] ) + ( _V.z * m [ 8 ] ) + m [ 12 ];
        vtx.y = ( _V.x * m [ 1 ] ) + ( _V.y * m [ 5 ] ) + ( _V.z * m [ 9 ] ) + m [ 13 ];
        vtx.z = ( _V.x * m [ 2 ] ) + ( _V.y * m [ 6 ] ) + ( _V.z * m [ 10 ] ) + m [ 14 ];

        return vtx;
    }

    // Overloaded operators

    public static Matrix operator *( Matrix _A, Matrix _B )
    {
        Matrix wk = new Matrix();

        wk.m [ 0 ] = _A.m [ 0 ] * _B.m [ 0 ] + _A.m [ 4 ] * _B.m [ 1 ] + _A.m [ 8 ] * _B.m [ 2 ] + _A.m [ 12 ] * _B.m [ 3 ];
        wk.m [ 4 ] = _A.m [ 0 ] * _B.m [ 4 ] + _A.m [ 4 ] * _B.m [ 5 ] + _A.m [ 8 ] * _B.m [ 6 ] + _A.m [ 12 ] * _B.m [ 7 ];
        wk.m [ 8 ] = _A.m [ 0 ] * _B.m [ 8 ] + _A.m [ 4 ] * _B.m [ 9 ] + _A.m [ 8 ] * _B.m [ 10 ] + _A.m [ 12 ] * _B.m [ 11 ];
        wk.m [ 12 ] = _A.m [ 0 ] * _B.m [ 12 ] + _A.m [ 4 ] * _B.m [ 13 ] + _A.m [ 8 ] * _B.m [ 14 ] + _A.m [ 12 ] * _B.m [ 15 ];

        wk.m [ 1 ] = _A.m [ 1 ] * _B.m [ 0 ] + _A.m [ 5 ] * _B.m [ 1 ] + _A.m [ 9 ] * _B.m [ 2 ] + _A.m [ 13 ] * _B.m [ 3 ];
        wk.m [ 5 ] = _A.m [ 1 ] * _B.m [ 4 ] + _A.m [ 5 ] * _B.m [ 5 ] + _A.m [ 9 ] * _B.m [ 6 ] + _A.m [ 13 ] * _B.m [ 7 ];
        wk.m [ 9 ] = _A.m [ 1 ] * _B.m [ 8 ] + _A.m [ 5 ] * _B.m [ 9 ] + _A.m [ 9 ] * _B.m [ 10 ] + _A.m [ 13 ] * _B.m [ 11 ];
        wk.m [ 13 ] = _A.m [ 1 ] * _B.m [ 12 ] + _A.m [ 5 ] * _B.m [ 13 ] + _A.m [ 9 ] * _B.m [ 14 ] + _A.m [ 13 ] * _B.m [ 15 ];

        wk.m [ 2 ] = _A.m [ 2 ] * _B.m [ 0 ] + _A.m [ 6 ] * _B.m [ 1 ] + _A.m [ 10 ] * _B.m [ 2 ] + _A.m [ 14 ] * _B.m [ 3 ];
        wk.m [ 6 ] = _A.m [ 2 ] * _B.m [ 4 ] + _A.m [ 6 ] * _B.m [ 5 ] + _A.m [ 10 ] * _B.m [ 6 ] + _A.m [ 14 ] * _B.m [ 7 ];
        wk.m [ 10 ] = _A.m [ 2 ] * _B.m [ 8 ] + _A.m [ 6 ] * _B.m [ 9 ] + _A.m [ 10 ] * _B.m [ 10 ] + _A.m [ 14 ] * _B.m [ 11 ];
        wk.m [ 14 ] = _A.m [ 2 ] * _B.m [ 12 ] + _A.m [ 6 ] * _B.m [ 13 ] + _A.m [ 10 ] * _B.m [ 14 ] + _A.m [ 14 ] * _B.m [ 15 ];

        wk.m [ 3 ] = _A.m [ 3 ] * _B.m [ 0 ] + _A.m [ 7 ] * _B.m [ 1 ] + _A.m [ 11 ] * _B.m [ 2 ] + _A.m [ 15 ] * _B.m [ 3 ];
        wk.m [ 7 ] = _A.m [ 3 ] * _B.m [ 4 ] + _A.m [ 7 ] * _B.m [ 5 ] + _A.m [ 11 ] * _B.m [ 6 ] + _A.m [ 15 ] * _B.m [ 7 ];
        wk.m [ 11 ] = _A.m [ 3 ] * _B.m [ 8 ] + _A.m [ 7 ] * _B.m [ 9 ] + _A.m [ 11 ] * _B.m [ 10 ] + _A.m [ 15 ] * _B.m [ 11 ];
        wk.m [ 15 ] = _A.m [ 3 ] * _B.m [ 12 ] + _A.m [ 7 ] * _B.m [ 13 ] + _A.m [ 11 ] * _B.m [ 14 ] + _A.m [ 15 ] * _B.m [ 15 ];

        return wk;
    }
}