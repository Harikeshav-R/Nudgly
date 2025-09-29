#include <QGuiApplication>
#include <QQmlApplicationEngine>
#include <QQmlContext>

#include "ViewModels/MainViewModel.h"
#include "Services/WindowPrivacyService.h"


int main(int argc, char* argv[])
{
    QGuiApplication application(argc, argv);
    QGuiApplication::setApplicationName("Nudgly");
    QGuiApplication::setOrganizationName("Nudgly");
    QCoreApplication::setOrganizationDomain("Nudgly.com");

    QQmlApplicationEngine engine;

    Models::ConversationModel::ConversationModel conversationModel;
    MainViewModel mainViewModel(&conversationModel);

    engine.rootContext()->setContextProperty("mainViewModel", &mainViewModel);

    engine.load(QUrl("qrc:/Views/Main.qml"));
    engine.load(QUrl("qrc:/Views/Answer.qml"));

    if (engine.rootObjects().isEmpty())
    {
        exit(EXIT_FAILURE);
    }

    Services::WindowPrivacyService::enable(&engine);

    return QGuiApplication::exec();
}
