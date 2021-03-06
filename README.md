# SpectrumArchiveReader

Программа читает диски 5.25" формата TR-DOS, CP/M и IS-DOS через драйвер fdrawcmd.sys (https://simonowen.com/fdrawcmd/). 

Что может делать:

• Читать диски (5.25'' Double Density) форматов TR-DOS, IS-DOS и CP/M посекторно, до 172 треков если надо.
• Дочитывать только непрочитанные сектора. Очень экономит время на плохо читающихся дисках.
• Читать только указанный диапазон треков на диске. Позволяет пропустить области диска которые заведомо нечитаемы и сразу сосредоточиться на указанном треке у которого есть потенциал прочитаться.
• Читать диски в обратном направлении. Как ни странно, у меня это улучшало чтение.
• Читать сектора в случайном порядке. В некоторых случаях улучшает чтение.
• Сохраняет образы в TRD. Можно позже загрузить TRD-образ и снова попытаться прочитать только bad-сектора.
При сохранении образа в TRD его bad-сектора заполняются символом B (как это делает WinTRD). Необработанные сектора (у которых не было попытки чтения) заполняются символом N. При загрузке образа по этим меткам определяются плохие и еще не читавшиеся сектора.
• Может читать верхнюю сторону двумя способами: задавать верхнюю сторону равной 0 либо 1. Есть также автоопределение параметра Head верхней стороны.
• Может читать только одну сторону: верхнюю или нижнюю, либо обе стороны. Практика показала что иногда бывает быстрее прочитать диск по сторонам отдельно, потому что верхняя сторона может быть отформатирована разными способами и должна читаться с разными настройками. У меня были диски где формат менялся много раз. Так получалось потому что я мог форматировать диск много раз и делать это разными программами и каждый раз не до конца, т.е. прерывая форматирование, поэтому разные виды форматов могли таким образом наложиться. Этой программой можно прочитать отдельные области верхней стороны диска указывая формат для каждой области. Позже, добавленное автоопределение параметра Head еще больше упростило эту задачу.
