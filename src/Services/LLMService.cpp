#include "LLMService.h"
#include "../Constants.h"
#include "../Services/SettingsService.h"

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

LLMService::LLMService(QObject* parent)
    : QObject(parent),
      m_networkManager(new QNetworkAccessManager(this))
{
    m_apiKey = SettingsService::readValue("LLM/apiKey").toString();
}

QString LLMService::toBase64Png(const QPixmap& pixmap)
{
    QByteArray byteArray;
    QBuffer buffer(&byteArray);
    buffer.open(QIODevice::WriteOnly);
    pixmap.save(&buffer, "PNG");
    return QString::fromUtf8(byteArray.toBase64());
}

QString LLMService::takeScreenshot()
{
    QScreen* screen = QGuiApplication::primaryScreen();
    if (!screen)
    {
        qCritical() << "Unable to get primary screen.";
        return QString();
    }
    const QPixmap pixmap = screen->grabWindow(0);
    return toBase64Png(pixmap);
}

void LLMService::generateAnswer(Models::ConversationModel::ConversationModel* conversationModel)
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

    sendApiRequest(userMessage, conversationModel);
}

void LLMService::sendApiRequest(const Models::ConversationModel::Message& userMessage,
                                Models::ConversationModel::ConversationModel* conversationModel)
{
    auto [model, messages] = conversationModel->getConversationData();
    messages.append(userMessage);

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

    QNetworkRequest request(Constants::MODEL_ENDPOINT);
    request.setRawHeader("Authorization", ("Bearer " + m_apiKey).toUtf8());
    request.setHeader(QNetworkRequest::ContentTypeHeader, "application/json");

    qInfo() << "Sending request to API...";
    QNetworkReply* reply = m_networkManager->post(request, QJsonDocument(payloadObject).toJson());

    connect(reply, &QNetworkReply::finished, this,
            [this, reply, conversationModel, userMessage]
            {
                handleApiResponse(reply, conversationModel, userMessage);
            });
}

void LLMService::handleApiResponse(QNetworkReply* reply,
                                   Models::ConversationModel::ConversationModel* conversationModel,
                                   const Models::ConversationModel::Message& userMessage)
{
    if (reply->error() != QNetworkReply::NoError)
    {
        const QString errorString = reply->errorString();
        const QByteArray responseBody = reply->readAll();
        qCritical() << QString("Error making API request: %1").arg(errorString);
        qInfo() << QString("Response Body: %1").arg(QString::fromUtf8(responseBody));
        emit errorOccurred(errorString);
    }
    else
    {
        qInfo() << ("Request successful.");

        if (const QJsonDocument doc = QJsonDocument::fromJson(reply->readAll()); doc.isNull() || !doc.isObject())
        {
            const QString errorMsg = "Error: Failed to parse JSON response.";
            qFatal() << errorMsg;
            emit errorOccurred(errorMsg);
        }
        else
        {
            // Add user message to the model first
            QVariantList userContentList;
            for (const auto& part : userMessage.content)
            {
                userContentList.append(part.toMap());
            }
            conversationModel->addMessage(userMessage.role, userContentList);

            // Extract assistant response and add it to the model
            const QString assistantText = doc.object()["choices"].toArray()[0]
                .toObject()["message"]
                .toObject()["content"].toString();
            const QString assistantRole = doc.object()["choices"].toArray()[0]
                .toObject()["message"]
                .toObject()["role"].toString();

            QVariantMap textPartMap;
            textPartMap["type"] = "text";
            textPartMap["text"] = assistantText;
            conversationModel->addMessage(assistantRole, {textPartMap});

            emit answerGenerated(assistantText);
        }
    }

    reply->deleteLater();
}
