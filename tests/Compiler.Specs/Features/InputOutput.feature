#language: ru
Функциональность: ввод-вывод и завершение программы

    Сценарий: вывод чисел функцией printi
        Пусть я скомпилировал программу "features/input_output/print_int.tig"

        Когда я выполняю программу

        Тогда я увижу вывод:
        """
        1337
        """

    Сценарий: вывод текста функцией print
        Пусть я скомпилировал программу "features/input_output/print_string.tig"

        Когда я выполняю программу

        Тогда я увижу вывод:
        """
        Tyger Tyger, burning bright,
        In the forests of the night;
        What immortal hand or eye,
        Could frame thy fearful symmetry?
        """

    Сценарий: вывод текста и вызов функции flush
        Пусть я скомпилировал программу "features/input_output/print_string_with_flush.tig"

        Когда я выполняю программу

        Тогда я увижу вывод:
        """
        Tyger Tyger, burning bright,
        In the forests of the night;
        What immortal hand or eye,
        Could frame thy fearful symmetry?
        """

    Сценарий: завершение из программы функцией exit
        Пусть я скомпилировал программу "features/input_output/exit.tig"

        Когда я выполняю программу

        Тогда я увижу вывод:
        """
        13
        """

    Сценарий: завершение из программы функцией exit с ненулевым кодом
        Пусть я скомпилировал программу "features/input_output/exit_with_non_zero_code.tig"

        Когда я выполняю программу с перехватом ошибок

        Тогда я получу код возврата 128
        Тогда я увижу вывод:
        """
        before exit
        """

    Сценарий: посимвольное чтение ввода
        Пусть я скомпилировал программу "features/input_output/getchar_5_times.tig"

        Когда я ввожу текст:
            """
            Tyger, Tyger
            """
        И выполняю программу

        Тогда я увижу вывод:
        """
        Tyger
        """
        