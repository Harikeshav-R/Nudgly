#ifndef NUDGLY_LLMSERVICE_H
#define NUDGLY_LLMSERVICE_H

#include <QNetworkAccessManager>
#include <QSettings>

#include "Models/ConversationModel.h"


namespace Services
{
    class LLMService final : public QObject
    {
        Q_OBJECT

    public:
        explicit LLMService(QObject* parent = nullptr);

    signals:
        void answerGenerated(const QString& answer);
        void errorOccurred(const QString& error);
        void answerChunkReceived(const QString& chunk);

    public slots:
        void generateAnswer(Models::ConversationModel::ConversationModel* conversationModel);

    private slots:
        void handleApiStream();
        void handleApiFinished();

    private:
        // Static helper functions
        static QString toBase64Png(const QPixmap& pixmap);
        static QString takeScreenshot();

        // Private methods
        void sendApiRequest(Models::ConversationModel::ConversationModel* conversationModel);

        // Member variables
        QString m_apiKey;
        QNetworkAccessManager* m_networkManager;
        QSettings m_settings;

        QNetworkReply* m_currentReply = nullptr; // Keep track of the current reply
        Models::ConversationModel::ConversationModel* m_currentModel = nullptr; // And the current model

        QByteArray m_streamBuffer; // Buffer for incoming data chunks
        QString m_fullResponse; // Buffer for the complete final answer
    };
}

#endif //NUDGLY_LLMSERVICE_H
