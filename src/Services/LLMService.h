#ifndef NUDGLY_LLMSERVICE_H
#define NUDGLY_LLMSERVICE_H

#include <QNetworkAccessManager>
#include <QSettings>
#include "../Models/ConversationModel.h"


class LLMService final : public QObject
{
    Q_OBJECT

public:
    explicit LLMService(QObject* parent = nullptr);

signals:
    void answerGenerated(const QString& answer);
    void errorOccurred(const QString& error);

public slots:
    void generateAnswer(Models::ConversationModel::ConversationModel* conversationModel);

private slots:
    void handleApiResponse(QNetworkReply* reply,
                           Models::ConversationModel::ConversationModel* conversationModel,
                           const Models::ConversationModel::Message& userMessage);

private:
    // Static helper functions
    static QString toBase64Png(const QPixmap& pixmap);
    static QString takeScreenshot();

    // Private methods
    void sendApiRequest(const Models::ConversationModel::Message& userMessage,
                        Models::ConversationModel::ConversationModel* conversationModel);

    // Member variables
    QString m_apiKey;
    QNetworkAccessManager* m_networkManager;
    QSettings m_settings;
};


#endif //NUDGLY_LLMSERVICE_H
