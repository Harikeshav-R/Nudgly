#include <QGuiApplication>
#include <QQmlApplicationEngine>

int main(int argc, char* argv[])
{
    QGuiApplication application(argc, argv);
    QGuiApplication::setApplicationName("Nudgly");
    QGuiApplication::setOrganizationName("Nudgly");
    QCoreApplication::setOrganizationDomain("Nudgly.com");

    QQmlApplicationEngine engine;

    engine.load(QUrl("qrc:/views/main.qml"));

    if (engine.rootObjects().isEmpty())
    {
        exit(EXIT_FAILURE);
    }

    return QGuiApplication::exec();
}
