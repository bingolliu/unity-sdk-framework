#ifndef CommonIOSSdkHelper_h
#define CommonIOSSdkHelper_h

#import <Foundation/Foundation.h>
#import "IOSSdkHelper.h"

/**
 * 未接入渠道 SDK 时的默认实现，同时作为接入新渠道的模板。
 */
@interface CommonIOSSdkHelper : NSObject <IOSSdkHelper>
@end

#endif /* CommonIOSSdkHelper_h */