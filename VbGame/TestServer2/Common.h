
#ifndef COMMON_H
#define COMMON_H
#pragma once

#include "targetver.h"

#define WIN32_LEAN_AND_MEAN             //  ´Ó Windows Í·ÎÄ¼þÖÐÅÅ³ý¼«ÉÙÊ¹ÓÃµÄÐÅÏ¢
// Windows Í·ÎÄ¼þ:
#include <windows.h>
#include <WinSock2.h>
#include <ws2tcpip.h>
#include <process.h>

// C ÔËÐÐÊ±Í·ÎÄ¼þ
#include <stdlib.h>
#include <malloc.h>
#include <memory.h>
#include <tchar.h>

typedef __int16 int16;
typedef __int32 int32;
typedef __int64 int64;
typedef unsigned __int16 uint16;
typedef unsigned __int32 uint32;
typedef unsigned __int64 uint64;

typedef uint16 ushort;
typedef uint32 uint;
typedef uint64 ulong;
typedef unsigned long long BatchHandle;


#include <cassert>
#include <windows.h>

#pragma warning(push)
#pragma warning(disable:4251)
#pragma warning(disable:4244)
#include <string>
#include <vector>
#include <list>
#include <map>

#include <queue>
#include <deque>
#include <unordered_set>
#include <unordered_map>

#include <algorithm>
#include <functional>
#include <limits>

#include <fstream>
#include <iostream>
#include <iomanip>
#include <sstream>




//#include "FastDelegate\FastDelegate.h"
#pragma warning(pop)

#define FAIL_RET(x) do { assert(x); \
	if( FAILED( hr = ( x  ) ) ) \
	return hr; } while(0)


typedef std::wstring String;



static const int32 MaxInt32 = 0x7fffffff;



struct Size
{
	int32 Width, Height;
	Size(){}
	Size(const int32 width, const int height) { Width = width; Height = height; }

	inline bool operator == ( const Size& value ) const
	{
		return ( Width == value.Width && Height == value.Height );
	}

	inline bool operator != ( const Size& value ) const
	{
		return ( Width != value.Width || Height != value.Height );
	}
};


static void DecodeString(String& s)
{
	for (uint i=0;i<s.length();i++)
	{
		if (s[i]=='`')
		{
			s[i] = ' ';
		}
	}
}
static void EncodeString(String& s)
{
	for (uint i=0;i<s.length();i++)
	{
		if (s[i]==' ')
		{
			s[i] = '`';
		}
	}
}


#endif