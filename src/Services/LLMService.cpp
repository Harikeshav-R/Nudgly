#include "Services/LLMService.h"

#include <QScreen>
#include <QBuffer>
#include <QNetworkAccessManager>
#include <QNetworkRequest>
#include <QNetworkReply>
#include <QJsonObject>
#include <QJsonArray>
#include <QVariantMap>
#include <QPixMap>
#include <QtLogging>

#include "Constants.h"
#include "Services/SettingsService.h"


// --- JSON Serialization Helpers ---
namespace
{
    QJsonObject toJson(const Models::ConversationModel::ImageUrl& imageUrl)
    {
        return {{"url", imageUrl.url}};
    }

    QJsonObject toJson(const Models::ConversationModel::TextPart& textPart)
    {
        return {{"type", "text"}, {"text", textPart.text}};
    }

    QJsonObject toJson(const Models::ConversationModel::ImageUrlPart& imagePart)
    {
        return {{"type", "image_url"}, {"image_url", toJson(imagePart.imageUrl)}};
    }

    QJsonArray toJson(const QList<Models::ConversationModel::ContentPart>& contentList)
    {
        QJsonArray contentArray;
        for (const auto& partVariant : contentList)
        {
            if (partVariant.canConvert<Models::ConversationModel::TextPart>())
            {
                contentArray.append(toJson(partVariant.value<Models::ConversationModel::TextPart>()));
            }
            else if (partVariant.canConvert<Models::ConversationModel::ImageUrlPart>())
            {
                contentArray.append(toJson(partVariant.value<Models::ConversationModel::ImageUrlPart>()));
            }
        }
        return contentArray;
    }
} // end anonymous namespace

Services::LLMService::LLMService(QObject* parent)
    : QObject(parent),
      m_networkManager(new QNetworkAccessManager(this))
{
    m_apiKey = SettingsService::readValue("LLM/apiKey").toString();
}

QString Services::LLMService::toBase64Png(const QPixmap& pixmap)
{
    QByteArray byteArray;
    QBuffer buffer(&byteArray);
    buffer.open(QIODevice::WriteOnly);
    pixmap.save(&buffer, "PNG");
    return QString::fromUtf8(byteArray.toBase64());
}

QString Services::LLMService::takeScreenshot()
{
    QScreen* screen = QGuiApplication::primaryScreen();
    if (!screen)
    {
        qCritical() << "Unable to get primary screen.";
        return {};
    }
    const QPixmap pixmap = screen->grabWindow(0);
    return toBase64Png(pixmap);
}

void Services::LLMService::generateAnswer(Models::ConversationModel::ConversationModel* conversationModel)
{
    const QString base64Screenshot = takeScreenshot();
    if (base64Screenshot.isEmpty())
    {
        const QString errorMsg = "Capturing screen failed.";
        qCritical() << errorMsg;
        emit errorOccurred(errorMsg);
        return;
    }

    Models::ConversationModel::TextPart textPart;
    textPart.text = Constants::DEFAULT_PROMPT;

    Models::ConversationModel::ImageUrlPart imagePart;
    imagePart.imageUrl.url = QString("data:image/png;base64,%1").arg(base64Screenshot);

    Models::ConversationModel::Message userMessage;
    userMessage.role = "user";
    userMessage.content.append(QVariant::fromValue(textPart));
    userMessage.content.append(QVariant::fromValue(imagePart));

    conversationModel->addMessage(userMessage);

    Models::ConversationModel::Message assistantPlaceholder;
    assistantPlaceholder.role = "assistant";
    conversationModel->addMessage(assistantPlaceholder);

    sendApiRequest(conversationModel);
}

void Services::LLMService::sendApiRequest(
    Models::ConversationModel::ConversationModel* conversationModel)
{
    auto [model, messages] = conversationModel->getConversationData();
    // messages.append(userMessage);

    // Create the final JSON payload from the Conversation object
    QJsonObject payloadObject;
    payloadObject["model"] = model;
    QJsonArray messagesArray;
    for (const auto& [role, content] : messages)
    {
        QJsonObject msgObject;
        msgObject["role"] = role;
        msgObject["content"] = toJson(content);
        messagesArray.append(msgObject);
    }
    payloadObject["messages"] = messagesArray;
    payloadObject["stream"] = true;

    QNetworkRequest request(Constants::MODEL_ENDPOINT);
    request.setRawHeader("Authorization", ("Bearer " + m_apiKey).toUtf8());
    request.setHeader(QNetworkRequest::ContentTypeHeader, "application/json");

    m_fullResponse.clear();
    m_streamBuffer.clear();

    m_currentReply = m_networkManager->post(request, QJsonDocument(payloadObject).toJson());
    m_currentModel = conversationModel; // Store model for use in slots

    // Connect to the readyRead signal to process chunks as they arrive
    connect(m_currentReply, &QNetworkReply::readyRead, this, &LLMService::handleApiStream);

    // Connect to the finished signal for cleanup and finalization
    connect(m_currentReply, &QNetworkReply::finished, this, &LLMService::handleApiFinished);

    // Connect to the error signal for robust error handling
    connect(m_currentReply, &QNetworkReply::errorOccurred, this, [this](const QNetworkReply::NetworkError code)
    {
        qCritical() << "Network error occurred:" << code << m_currentReply->errorString();
        emit errorOccurred(m_currentReply->errorString());
    });
}

void Services::LLMService::handleApiStream()
{
    // Append new data from the reply to our buffer
    m_streamBuffer.append(m_currentReply->readAll());

    // Process the buffer line by line. SSE events are separated by "\n\n".
    QList<QByteArray> events = m_streamBuffer.split('\n');
    m_streamBuffer = events.takeLast(); // Keep incomplete line in buffer

    for (const QByteArray& event : events)
    {
        if (event.startsWith("data: "))
        {
            QByteArray jsonData = event.mid(6);
            if (jsonData.trimmed() == "[DONE]")
            {
                continue; // The 'finished' signal will handle the end
            }

            QJsonDocument doc = QJsonDocument::fromJson(jsonData);
            if (!doc.isObject()) continue;

            // Extract the text chunk from the delta
            const QString chunk = doc.object()["choices"].toArray()[0]
                .toObject()["delta"]
                .toObject()["content"].toString();

            if (!chunk.isEmpty())
            {
                m_fullResponse.append(chunk);
                m_currentModel->appendToLastMessage(chunk);
                emit answerChunkReceived(chunk);
            }
        }
    }
}

void Services::LLMService::handleApiFinished()
{
    if (m_currentReply->error() == QNetworkReply::NoError)
    {
        // Handle any remaining data in the buffer
        handleApiStream();
        qInfo() << "API stream finished successfully.";
        emit answerGenerated(m_fullResponse);
    }
    else
    {
        // Error signal will have already been emitted.
        qCritical() << "API stream finished with error:" << m_currentReply->errorString();
    }

    m_currentReply->deleteLater();
    m_currentReply = nullptr;
    m_currentModel = nullptr;
}
