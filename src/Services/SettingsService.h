#ifndef NUDGLY_SETTINGSSERVICE_H
#define NUDGLY_SETTINGSSERVICE_H

#include <QVariant>

class SettingsService
{
public:
    // Delete constructor and destructor to prevent instantiation.
    SettingsService() = delete;
    ~SettingsService() = delete;

    static void writeValue(const QString& key, const QVariant& value);

    static QVariant readValue(const QString& key, const QVariant& defaultValue = QVariant());

private:
    static void initializeDefaults();
};


#endif //NUDGLY_SETTINGSSERVICE_H
