#import "UnityAppController.h"
@interface AuthUnityAppController : UnityAppController
void UnitySendMessage( const char * className, const char * methodName, const char * param );
@end
@implementation AuthUnityAppController
- (BOOL) application:(UIApplication*)application
            openURL:(NSURL*)url
            options:(nonnull NSDictionary<UIApplicationOpenURLOptionsKey,id> *)options
{
    NSLog(@"url received %@",url);
    // call the parent implementation
    [super application:application openURL:url options:options];
    if (!url)
        return NO;

    NSString* uriScheme = @"mlxr-app";
    NSString* uriHost = @"auth-callback";
    
    if ([url.scheme isEqualToString:uriScheme]) {
        if ([url.host isEqualToString:uriHost]) {
            if (url.query) {
                const char * queryString = [url.query UTF8String];
                NSLog(@"received auth reply with query string");
                UnitySendMessage("SignInCanvas", "OnAuthReply", queryString);
            } else {
                NSLog(@"received auth reply with no query string");
                UnitySendMessage("SignInCanvas", "OnAuthReply", "");
            }
        } else {
            NSLog(@"received unexpected url host: [%@]", url.host);
        }
    } else {
        NSLog(@"received unexpected url scheme: [%@]", url.scheme);
    }

    return YES;
}
@end
IMPL_APP_CONTROLLER_SUBCLASS(AuthUnityAppController)
