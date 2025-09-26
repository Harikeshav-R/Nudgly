//
// Created by Harikeshav R on 9/25/25.
//

#ifndef NUDGLY_CONVERSATION_MODEL_H
#define NUDGLY_CONVERSATION_MODEL_H

#include <QList>
#include <QString>
#include <QAbstractListModel>

namespace Models::ConversationModel
{
    struct ImageUrl
    {
        QString url;
    };

    struct TextPart
    {
        QString type = "text";
        QString text;
    };

    struct ImageUrlPart
    {
        QString type = "image_url";
        ImageUrl imageUrl;
    };

    using ContentPart = QVariant;

    struct Message
    {
        QString role;
        QList<ContentPart> content;
    };

    struct Conversation
    {
        QString model;
        QList<Message> messages;
    };

    class ConversationModel final : public QAbstractListModel
    {
        Q_OBJECT
        Q_PROPERTY(QString modelName READ modelName NOTIFY modelNameChanged)

    public:
        enum Roles
        {
            RoleRole = Qt::UserRole + 1,
            ContentRole
        };

        explicit ConversationModel(QObject* parent = nullptr);

        // --- Public API for Modifying the Model ---
        [[nodiscard]] const Conversation& getConversationData() const;

    public slots:
        void addMessage(const QString& role, const QVariantList& content);
        void clear();
        void setModelName(const QString& modelName);

    signals:
        void modelNameChanged();

    protected:
        // --- QAbstractListModel Overrides ---
        [[nodiscard]] int rowCount(const QModelIndex& parent = QModelIndex()) const override;
        [[nodiscard]] QVariant data(const QModelIndex& index, int role = Qt::DisplayRole) const override;
        [[nodiscard]] QHash<int, QByteArray> roleNames() const override;

    private:
        [[nodiscard]] QString modelName() const;

        Conversation m_conversation;
    };
}

Q_DECLARE_METATYPE(Models::ConversationModel::TextPart)

Q_DECLARE_METATYPE(Models::ConversationModel::ImageUrlPart)

#endif //NUDGLY_CONVERSATION_MODEL_H
