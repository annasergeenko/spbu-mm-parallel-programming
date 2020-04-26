class ChannelClosedException: Exception() {
    override val message: String = "Channel was closed"
}