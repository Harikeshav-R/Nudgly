#include "SettingsService.h"

#include <QSettings>
#include <mutex> // For std::once_flag

#include "../Constants.h"

namespace
{
    // std::once_flag ensures initializeDefaults() is called only once
    // across all threads during the application's lifetime.
    std::once_flag initFlag;
}

void SettingsService::writeValue(const QString& key, const QVariant& value)
{
    // Ensure defaults are set before writing.
    std::call_once(initFlag, &SettingsService::initializeDefaults);

    QSettings settings;
    settings.setValue(key, value);
}

QVariant SettingsService::readValue(const QString& key, const QVariant& defaultValue)
{
    // Ensure defaults are set before reading.
    std::call_once(initFlag, &SettingsService::initializeDefaults);

    const QSettings settings;
    return settings.value(key, defaultValue);
}

void SettingsService::initializeDefaults()
{
    // Check for a sentinel key. If it doesn't exist, we know it's the first run.
    if (QSettings settings; !settings.contains("initialized"))
    {
        // Write the sentinel key first to prevent re-entry in case of a crash.
        settings.setValue("initialized", true);

        // --- Set application default values ---
        settings.setValue("LLM/apiKey", QString::fromUtf8(getenv("OPENAI_API_KEY")));
        settings.setValue("LLM/modelName", Constants::MODEL_NAME);
        settings.setValue("LLM/systemPrompt", Constants::DEFAULT_PROMPT);
    }
}
