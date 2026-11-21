export enum ToasterSuccessEventMessage {
    default_value = 'Sucesso',
    join_event = 'Entrada cadastrada!',
    leave_event = 'Você saiu do evento',
    report_user = 'Denúncia enviada!',
    rating_user = 'Avaliação enviada!',
    new_event = 'Evento criado!',
    edited_event = 'Evento editado!'
}

export enum ToasterErrorEventMessage {
    report_user = 'Erro ao denunciar o usuário!',
    rating_user = 'Erro ao avaliar o usuário!'
}

export enum ToasterSuccessSettingsMessage {
    save_changes = 'Usuário editadao com sucesso!'
}