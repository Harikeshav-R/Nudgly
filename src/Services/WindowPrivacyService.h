#ifndef NUDGLY_WINDOW_PRIVACY_SERVICE_H
#define NUDGLY_WINDOW_PRIVACY_SERVICE_H

#include <QQuickWindow>
#import <QQmlApplicationEngine>

namespace Services::WindowPrivacyService
{
    void disable(const QQuickWindow* window);
    void disable(const QQmlApplicationEngine* engine);

    void enable(const QQuickWindow* window);
    void enable(const QQmlApplicationEngine* engine);
}

#endif //NUDGLY_WINDOW_PRIVACY_SERVICE_H
