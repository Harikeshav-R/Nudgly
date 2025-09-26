#ifndef NUDGLY_WINDOW_PRIVACY_SERVICE_H
#define NUDGLY_WINDOW_PRIVACY_SERVICE_H

#include <QQuickWindow>

namespace Services::WindowPrivacyService
{
    void disable(const QQuickWindow* window);
    void enable(const QQuickWindow* window);
}

#endif //NUDGLY_WINDOW_PRIVACY_SERVICE_H
