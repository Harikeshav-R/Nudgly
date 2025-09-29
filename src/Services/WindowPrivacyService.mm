#include "Services/WindowPrivacyService.h"

void Services::WindowPrivacyService::enable(const QQmlApplicationEngine* engine)
{
    QList<QObject *> rootObjects = engine->rootObjects();
    QList<QQuickWindow *> qQuickWindows;

    for (QObject *obj: rootObjects)
    {
        auto *window = qobject_cast<QQuickWindow *>(obj);
        if (window)
        {
            qQuickWindows.append(window);
        }
    }

    for (QQuickWindow *window: qQuickWindows)
    {
        enable(window);
    }
}

void Services::WindowPrivacyService::disable(const QQmlApplicationEngine* engine)
{
    QList<QObject *> rootObjects = engine->rootObjects();
    QList<QQuickWindow *> qQuickWindows;

    for (QObject *obj: rootObjects)
    {
        auto *window = qobject_cast<QQuickWindow *>(obj);
        if (window)
        {
            qQuickWindows.append(window);
        }
    }

    for (QQuickWindow *window: qQuickWindows)
    {
        disable(window);
    }
}

#if defined(_WIN32) || defined(__WIN32__) || defined(WIN32)

#include <windows.h>

void Services::WindowPrivacyService::enable(const QQuickWindow* window)
{
    const auto hwnd = reinterpret_cast<HWND>(window->winId());

    try
    {
        SetWindowDisplayAffinity(hwnd, WDA_EXCLUDEFROMCAPTURE);
    }
    catch (...)
    {
        SetWindowDisplayAffinity(hwnd, WDA_MONITOR);
    }
}

void Services::WindowPrivacyService::disable(const QQuickWindow* window)
{
    const auto hwnd = reinterpret_cast<HWND>(window->winId());
    SetWindowDisplayAffinity(hwnd, WDA_NONE);
}

#elif defined(__APPLE__)

#import <Cocoa/Cocoa.h>

void Services::WindowPrivacyService::enable(const QQuickWindow *window) {
    void *view_ptr = reinterpret_cast<void *>(window->winId());
    auto *ns_view = (__bridge NSView *) view_ptr;

    NSWindow *ns_window = [ns_view window];
    if (ns_window)
    {
        [ns_window setSharingType:NSWindowSharingNone];
    }
}

void Services::WindowPrivacyService::disable(const QQuickWindow *window) {
    void *view_ptr = reinterpret_cast<void *>(window->winId());
    auto *ns_view = (__bridge NSView *) view_ptr;

    NSWindow *ns_window = [ns_view window];
    if (ns_window)
    {
        [ns_window setSharingType:NSWindowSharingReadOnly];
    }
}

#endif

