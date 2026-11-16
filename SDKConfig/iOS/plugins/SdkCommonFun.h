#ifndef SdkCommonFun_h
#define SdkCommonFun_h

#import <Foundation/Foundation.h>
#include <stdlib.h>
#include <string.h>

#ifdef __cplusplus
extern "C" {
#endif

/** C# DllImport 传入的 char* 转 NSString */
static inline NSString *CreateNSString(const char *string)
{
    return (string != NULL) ? [NSString stringWithUTF8String:string] : [NSString stringWithUTF8String:""];
}

/**
 * NSString 转 char*（堆内存）。
 * Unity 的 string 返回值由 mono/il2cpp 拷贝一份，因此这里的副本不会被释放，
 * 保持 Unity 官方示例写法；如果调用非常频繁，可改为静态缓冲区并自行控制长度。
 */
static inline char *_makeStringCopy(const char *string)
{
    if (string == NULL)
    {
        return NULL;
    }
    size_t length = strlen(string) + 1;
    char *result = (char *)malloc(length);
    strcpy(result, string);
    return result;
}

static inline char *_makeStringCopyFromNSString(NSString *string)
{
    return _makeStringCopy(string != nil ? [string UTF8String] : "");
}

#ifdef __cplusplus
}
#endif

#endif /* SdkCommonFun_h */