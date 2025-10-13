#include "Models/ConversationModel.h"

#include "Constants.h"
#include "Services/SettingsService.h"

void addSystemMessage(Models::ConversationModel::Conversation* conversation,
                      const QString& systemPrompt = Constants::DEFAULT_PROMPT,
                      const QString& modelName = Constants::MODEL_NAME)
{
    // Set up the initial system message
    Models::ConversationModel::TextPart systemTextPart;
    systemTextPart.text = systemPrompt;

    Models::ConversationModel::Message systemMessage;
    systemMessage.role = "system";
    systemMessage.content.append(QVariant::fromValue(systemTextPart));

    conversation->model = modelName;
    conversation->messages.append(systemMessage);
}

Models::ConversationModel::ConversationModel::ConversationModel(QObject* parent)
    : QAbstractListModel(parent)
{
    // Register the custom types to be used in QVariant
    qRegisterMetaType<TextPart>();
    qRegisterMetaType<ImageUrlPart>();

    // Set up the initial system message
    addSystemMessage(&m_conversation,
                     Services::SettingsService::readValue("systemPrompt", Constants::DEFAULT_PROMPT).toString(),
                     Services::SettingsService::readValue("modelName", Constants::MODEL_NAME).toString()
    );
}

int Models::ConversationModel::ConversationModel::rowCount(const QModelIndex& parent) const
{
    // For list models, parent is always invalid.
    if (parent.isValid())
        return 0;
    return m_conversation.messages.count();
}

QVariant Models::ConversationModel::ConversationModel::data(const QModelIndex& index, const int role) const
{
    if (!index.isValid() || index.row() >= m_conversation.messages.count())
        return {};

    const auto& [roleString, content] = m_conversation.messages.at(index.row());

    switch (role)
    {
    case RoleRole:
        return roleString;
    case ContentRole:
        // Convert QList<QVariant> to QVariantList for QML
        return QVariant::fromValue(content);
    default: ;
    }

    return {};
}

QHash<int, QByteArray> Models::ConversationModel::ConversationModel::roleNames() const
{
    QHash<int, QByteArray> roles;
    roles[RoleRole] = "role";
    roles[ContentRole] = "content";
    return roles;
}

void Models::ConversationModel::ConversationModel::addMessage(const QString& role, const QVariantList& content)
{
    beginInsertRows(QModelIndex(), rowCount(), rowCount());

    Message newMessage;
    newMessage.role = role;

    // Convert the QVariantList from QML into our C++ types
    for (const QVariant& item : content)
    {
        if (QVariantMap map = item.toMap(); map.value("type").toString() == "text")
        {
            TextPart part;
            part.text = map.value("text").toString();
            newMessage.content.append(QVariant::fromValue(part));
        }
        else if (map.value("type").toString() == "image_url")
        {
            ImageUrlPart part;
            part.imageUrl.url = map.value("image_url").toMap().value("url").toString();
            newMessage.content.append(QVariant::fromValue(part));
        }
    }

    m_conversation.messages.append(newMessage);
    endInsertRows();
}

void Models::ConversationModel::ConversationModel::addMessage(const Message& message)
{
    beginInsertRows(QModelIndex(), rowCount(), rowCount());

    m_conversation.messages.append(message);

    endInsertRows();
}

void Models::ConversationModel::ConversationModel::clear()
{
    beginResetModel();
    m_conversation.messages.clear();

    addSystemMessage(&m_conversation, Constants::DEFAULT_PROMPT, Constants::MODEL_NAME);

    endResetModel();
}

void Models::ConversationModel::ConversationModel::setModelName(const QString& modelName)
{
    if (m_conversation.model != modelName)
    {
        m_conversation.model = modelName;
        emit modelNameChanged();
    }
}

QString Models::ConversationModel::ConversationModel::modelName() const
{
    return m_conversation.model;
}

const Models::ConversationModel::Conversation& Models::ConversationModel::ConversationModel::getConversationData() const
{
    return m_conversation;
}

void Models::ConversationModel::ConversationModel::appendToLastMessage(const QString& textChunk)
{
    if (m_conversation.messages.isEmpty())
    {
        return; // Nothing to append to
    }

    // Get a reference to the last message in the list

    if (auto& [role, content] = m_conversation.messages.last(); content.isEmpty())
    {
        // If the last message has no content, add a new text part
        TextPart newPart;
        newPart.text = textChunk;
        content.append(QVariant::fromValue(newPart));
    }
    else
    {
        // Find the last text part to append to
        // We iterate backwards to find the most recent text part
        for (int i = content.size() - 1; i >= 0; --i)
        {
            if (QVariant& partVariant = content[i]; partVariant.canConvert<TextPart>())
            {
                auto part = partVariant.value<TextPart>();
                part.text.append(textChunk);
                // Store the modified struct back into the QVariant
                partVariant.setValue(part);

                // Notify the view that the data for the last row has changed
                const QModelIndex lastIndex = index(rowCount() - 1, 0);
                emit dataChanged(lastIndex, lastIndex, {ContentRole});
                return; // Exit after appending
            }
        }
    }
}
