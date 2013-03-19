#ifndef _TEST_APP
#define _TEST_APP

//if standalone mode/non-addon
#define STANDALONE

//main
#include "ofxCvConstants.h"
//addon
#include "ofxNCore.h"

class HeightMap {
public:	
	float x, y, height;
	HeightMap()
	{
		this->x=0;
		this->y=0;
		this->height=0;
	}
	HeightMap(float x, float y, float height)
	{
		this->x = x;
		this->y = y;
		this->height = height;
	}
};

class Point2D
{
public:
	float x, y;
	Point2D() 
	{
		this->x=0; 
		this->y=0;
	}
	Point2D(float x, float y)
	{
		this->x = x;
		this->y = y;
	}

	bool operator==(const Point2D& _right)
    {
        return x == _right.x && y == _right.y;
    }

};

class Box2D
{
public:
	float x, y, width, height, widthstep;
	Box2D() 
	{
		this->x=0; 
		this->y=0;
		this->width=0;
		this->height=0;
		this->widthstep=0;
	}
	Box2D(float x, float y, float width, float height, float widthstep=0)
	{
		this->x = x;
		this->y = y;
		this->width=width;
		this->height=height;
		this->widthstep=widthstep;
	}
	
	virtual void  operator = ( Box2D box )
	{
		this->x = box.x;
		this->y = box.y;
		this->width= box.width;
		this->height= box.height;
		this->widthstep= box.widthstep;
	}

};

typedef vector<Point2D> Path2D;
typedef vector<Path2D> Paths;
typedef vector<HeightMap> CompHeightMap;
typedef vector<Box2D> Boxes;

class HeightData
{
public:
	//ID
	int ID;
	Box2D bbox;
	vector<float> heightPoints;
	HeightData()
	{
		this->ID=0;
		this->bbox.x = 0;
		this->bbox.y = 0;
		this->bbox.width = 0;
		this->bbox.height = 0;
	}
	HeightData(int _ID, Box2D _bbox, vector<float> _heightData)
	{
		this->ID = _ID;
		this->bbox = _bbox;
		this->heightPoints = _heightData;
	}
};


typedef vector<HeightData> BlobHeightMap;

class testApp : public ofBaseApp, public TouchListener
{
public:
	testApp() 
	{
		TouchEvents.addListener(this);
	}
	ofxNCoreVision * ccv;

	void setup();
	void update();
	void draw();

	void keyPressed(int key);
	void keyReleased(int key);
	void mouseMoved(int x, int y );
	void mouseDragged(int x, int y, int button);
	void mousePressed(int x, int y, int button);
	void mouseReleased();

	//Touch Events
	void TouchDown(Blob b);
	void TouchMoved(Blob b);
	void TouchUp(Blob b);

	//heightmap
	CompHeightMap generateHeightMap(IplImage *sourceImg, IplImage *processedImg, IplImage *heightImg); 
	CompHeightMap normalizeHeightMap(CompHeightMap inputMap); //normalize
	CompHeightMap reduceHeightMap(CompHeightMap inputMap, int reduceCount); //reduce poly count
	HeightMap getBoundPixelValue(CompHeightMap inputMap);
	float getPixelVariance(IplImage *sourceImg, Point2D startPnt ,int width, int height); //std dev.
	float getPixelDepth(IplImage *sourceImg, Point2D startPnt, int width, int height, Path2D polygon);
	int InsidePolygon(Path2D polygon, int N, Point2D p, int bound);
	Box2D getBoundingBox(Path2D inputPath);
	void sendHeightMap();
	//resampling and contours
	float getDistance(Point2D p1, Point2D p2);
	float pathLength(Path2D inputPath);
	Path2D resample(Path2D inputPath); 
	CompHeightMap gridMap(IplImage *sourceImg, IplImage *processedImg);
	void drawPath();
	//static path length
	int raPathLength;

	//vars 
	// this needs to be gray for camera
	//ofxCvGrayscaleImage raSourceImg;
	ofxCvGrayscaleImage raSourceImg;
	ofxCvGrayscaleImage raProcessedImg;
	IplImage *raHeightImg;
	IplImage *raIPLsource;
	IplImage *raIPLprocessed;

	float raCamWidth;
	float raCamHeight;
	int countHeight;
	int countWidth;

	CompHeightMap raHeightMap;
	CompHeightMap raNormalizedMap;
	CompHeightMap raReducedMap;
	BlobHeightMap raBlobHeightMap;
	
	int raBlobCount;
	int raContourSize;
	int raTotalSize;
	//communication
	bool raSendEnable;
	bool startHeightmap;
	bool startBlobs;
	bool startContour;
	bool startGraymap;
	bool startSimpleGrid;
	bool startCompleteGrid;
	int raFrameseq;
	int frameseq;

	ofxOscSender raSocket; 

	const char* raLocalHost;
	int	raPort;	
	int raHighestCount;
	void sendBlobs();
	void sendHeightBlobs();

	Paths outlines;
	Boxes bndBoxes;
};


#define PMIN(x,y) (x < y ? x : y)
#define PMAX(x,y) (x > y ? x : y)
#define __DBL_EPSILON__ 2.2204460492503131e-16

#endif

