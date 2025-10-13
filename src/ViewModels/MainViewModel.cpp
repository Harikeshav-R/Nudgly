#include "ViewModels/MainViewModel.h"

#include <QDebug>

MainViewModel::MainViewModel(Models::ConversationModel::ConversationModel* conversationModel, QObject* parent)
    : QObject(parent),
      m_conversationModel(conversationModel),
      m_llmService(new Services::LLMService(this)) // Instantiate the service
{
    // Connect service signals to our private handler slots
    connect(m_llmService, &Services::LLMService::answerChunkReceived, this, &MainViewModel::onAnswerChunkReceived);
    connect(m_llmService, &Services::LLMService::answerGenerated, this, &MainViewModel::onAnswerFinished);
    connect(m_llmService, &Services::LLMService::errorOccurred, this, &MainViewModel::onApiError);
}

// --- Public Slots ---

void MainViewModel::askAi()
{
    // This is non-blocking. It starts the task and returns immediately.
    setAnswerResult("");
    setIsThinking(true);
    m_llmService->generateAnswer(m_conversationModel);
}

void MainViewModel::toggleAnswersWindowVisibility()
{
    setAnswersWindowVisible(!m_answersWindowVisible);
}

void MainViewModel::openSettings()
{
    qDebug() << "Settings window requested";
}

// --- Private Slots ---

void MainViewModel::onAnswerChunkReceived(const QString& chunk)
{
    // Append the incoming text chunk to the current answer result.
    // The setter will emit the changed signal, updating the UI in real-time.
    setAnswerResult(m_answerResult + chunk);
}

void MainViewModel::onAnswerFinished(const QString& fullAnswer)
{
    // The stream has finished. Finalize the state.
    // This ensures the final text is consistent and, most importantly,
    // it updates the thinking state to stop any loading indicators in the UI.
    setAnswerResult(fullAnswer);
    setIsThinking(false);
}

void MainViewModel::onApiError(const QString& errorMessage)
{
    // Format the error message for the user
    setAnswerResult(QString("An error occurred: %1").arg(errorMessage));
    setIsThinking(false);
}

// --- Property Accessors ---

QString MainViewModel::answerResult() const
{
    return m_answerResult;
}

void MainViewModel::setAnswerResult(const QString& answerResult)
{
    if (m_answerResult != answerResult)
    {
        m_answerResult = answerResult;
        emit answerResultChanged();

        // Side-effect: show the answers window when a new result arrives
        if (answerResult.isEmpty())
        {
            setAnswersWindowVisible(true);
        }
    }
}

bool MainViewModel::answersWindowVisible() const
{
    return m_answersWindowVisible;
}

void MainViewModel::setAnswersWindowVisible(const bool visible)
{
    if (m_answersWindowVisible != visible)
    {
        m_answersWindowVisible = visible;
        emit answersWindowVisibilityChanged();
    }
}

bool MainViewModel::isThinking() const
{
    return m_isThinking;
}

void MainViewModel::setIsThinking(const bool thinking)
{
    if (m_isThinking != thinking)
    {
        m_isThinking = thinking;
        emit isThinkingChanged();
    }
}
