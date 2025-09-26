#ifndef NUDGLY_MAINVIEWMODEL_H
#define NUDGLY_MAINVIEWMODEL_H

#include <QObject>
#include <QString>

#include "../Models/ConversationModel.h"
#include "../Services/LLMService.h"

class MainViewModel final : public QObject
{
    Q_OBJECT

    // Properties exposed to QML
    Q_PROPERTY(QString answerResult READ answerResult WRITE setAnswerResult NOTIFY answerResultChanged)
    Q_PROPERTY(
        bool answersWindowVisible READ answersWindowVisible WRITE setAnswersWindowVisible NOTIFY
        answersWindowVisibilityChanged)
    Q_PROPERTY(bool isThinking READ isThinking WRITE setIsThinking NOTIFY isThinkingChanged)

public:
    explicit MainViewModel(Models::ConversationModel::ConversationModel* conversationModel, QObject* parent = nullptr);

    // Property getters
    QString answerResult() const;
    bool answersWindowVisible() const;
    bool isThinking() const;

public slots:
    // Public slots callable from the UI
    void askAi();
    void toggleAnswersWindowVisibility();
    void openSettings();

    // Property setters
    void setAnswerResult(const QString& answerResult);
    void setAnswersWindowVisible(bool visible);
    void setIsThinking(bool thinking);

signals:
    // Notification signals for properties
    void answerResultChanged();
    void answersWindowVisibilityChanged();
    void isThinkingChanged();

private slots:
    // Private slots for handling service signals
    void onAnswerReceived(const QString& assistantResponse);
    void onApiError(const QString& errorMessage);

private:
    // Member variables
    Models::ConversationModel::ConversationModel* m_conversationModel;
    LLMService* m_llmService;

    // Backing fields for properties
    QString m_answerResult;
    bool m_answersWindowVisible = false;
    bool m_isThinking = false;
};


#endif //NUDGLY_MAINVIEWMODEL_H
